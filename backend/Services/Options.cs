namespace NetTYB.Api.Services;

public record AuthCookieOptions(string RefreshCookieName, bool RefreshCookieSecure, int RefreshTokenTtlDays);

public record PasswordResetOptions(string BaseUrl);
