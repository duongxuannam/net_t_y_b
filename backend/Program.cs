using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using NetTYB.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

var jwtSecret = builder.Configuration["JWT_SECRET"];
if (string.IsNullOrWhiteSpace(jwtSecret) || jwtSecret.Length < 32)
{
    throw new InvalidOperationException("JWT_SECRET must be at least 32 characters long.");
}

var accessTokenTtlMinutes = builder.Configuration.GetValue("ACCESS_TOKEN_TTL_MIN", 15);
var refreshTokenTtlDays = builder.Configuration.GetValue("REFRESH_TOKEN_TTL_DAYS", 7);
var refreshCookieName = builder.Configuration.GetValue("REFRESH_COOKIE_NAME", "todo_refresh");
var refreshCookieSecure = builder.Configuration.GetValue("REFRESH_COOKIE_SECURE", builder.Environment.IsProduction());
var passwordResetUrlBase = builder.Configuration.GetValue("PASSWORD_RESET_URL_BASE", "http://localhost:5173/reset");
var passwordResetTtlMinutes = builder.Configuration.GetValue("PASSWORD_RESET_TTL_MIN", 30);
var allowedOrigins = builder.Configuration.GetValue("ALLOWED_ORIGINS", "http://localhost:3000,http://localhost:5173")
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
var rateLimitPerSecond = builder.Configuration.GetValue("RATE_LIMIT_PER_SECOND", 5);
var rateLimitBurst = builder.Configuration.GetValue("RATE_LIMIT_BURST", 10);

builder.Services.AddSingleton(new AuthService(jwtSecret, accessTokenTtlMinutes, refreshTokenTtlDays, passwordResetTtlMinutes));
builder.Services.AddSingleton(new AuthCookieOptions(refreshCookieName, refreshCookieSecure, refreshTokenTtlDays));
builder.Services.AddSingleton(new PasswordResetOptions(passwordResetUrlBase));
builder.Services.AddSingleton(new TodoService());

builder.Services.AddControllers();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("api", limiterOptions =>
    {
        limiterOptions.PermitLimit = rateLimitPerSecond;
        limiterOptions.Window = TimeSpan.FromSeconds(1);
        limiterOptions.QueueLimit = rateLimitBurst;
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseRateLimiter();
app.UseHttpsRedirection();
app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "NetTYB API v1");
    options.RoutePrefix = "api/docs";
});

var frontendDist = Path.GetFullPath(Path.Combine(app.Environment.ContentRootPath, "..", "frontend", "dist"));
if (Directory.Exists(frontendDist))
{
    app.UseDefaultFiles(new DefaultFilesOptions
    {
        FileProvider = new PhysicalFileProvider(frontendDist)
    });
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(frontendDist)
    });
}

app.MapControllers().RequireRateLimiting("api");

if (Directory.Exists(frontendDist))
{
    app.MapFallbackToFile("index.html", new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(frontendDist)
    });
}

app.MapGet("/", () => Results.Redirect("/api/health"));

app.Run();
