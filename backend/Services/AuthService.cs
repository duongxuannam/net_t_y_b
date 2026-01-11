using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using NetTYB.Api.Models;

namespace NetTYB.Api.Services;

public class AuthService
{
    private readonly byte[] _jwtKey;
    private readonly int _accessTokenTtlMinutes;
    private readonly int _refreshTokenTtlDays;
    private readonly int _passwordResetTtlMinutes;
    private readonly Dictionary<string, RefreshTokenRecord> _refreshTokens = new();
    private readonly Dictionary<string, ResetTokenRecord> _resetTokens = new();
    private readonly Dictionary<string, UserRecord> _users = new(StringComparer.OrdinalIgnoreCase);

    public AuthService(string jwtSecret, int accessTokenTtlMinutes, int refreshTokenTtlDays, int passwordResetTtlMinutes)
    {
        _jwtKey = Encoding.UTF8.GetBytes(jwtSecret);
        _accessTokenTtlMinutes = accessTokenTtlMinutes;
        _refreshTokenTtlDays = refreshTokenTtlDays;
        _passwordResetTtlMinutes = passwordResetTtlMinutes;
    }

    public AuthResponse Register(string email, string password)
    {
        if (_users.ContainsKey(email))
        {
            throw new InvalidOperationException("User already exists.");
        }

        var user = new UserRecord(Guid.NewGuid(), email, HashPassword(password));
        _users[email] = user;
        return IssueTokens(user);
    }

    public AuthResponse Login(string email, string password)
    {
        if (!_users.TryGetValue(email, out var user) || !VerifyPassword(password, user.PasswordHash))
        {
            throw new InvalidOperationException("Invalid credentials.");
        }

        return IssueTokens(user);
    }

    public AuthResponse Refresh(string refreshToken)
    {
        if (!_refreshTokens.TryGetValue(refreshToken, out var record) || record.ExpiresAt < DateTimeOffset.UtcNow)
        {
            throw new InvalidOperationException("Refresh token is invalid or expired.");
        }

        return IssueTokens(record.User);
    }

    public void Logout(string refreshToken)
    {
        _refreshTokens.Remove(refreshToken);
    }

    public string GeneratePasswordReset(string email)
    {
        if (!_users.ContainsKey(email))
        {
            throw new InvalidOperationException("User not found.");
        }

        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(_passwordResetTtlMinutes);
        _resetTokens[token] = new ResetTokenRecord(token, email, expiresAt);
        return token;
    }

    public void ResetPassword(string email, string token, string newPassword)
    {
        if (!_users.TryGetValue(email, out var user))
        {
            throw new InvalidOperationException("User not found.");
        }

        if (!_resetTokens.TryGetValue(token, out var resetRecord) || resetRecord.ExpiresAt < DateTimeOffset.UtcNow)
        {
            throw new InvalidOperationException("Reset token is invalid or expired.");
        }

        if (!string.Equals(resetRecord.Email, email, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Reset token does not match the user.");
        }

        _users[email] = user with { PasswordHash = HashPassword(newPassword) };
        _resetTokens.Remove(token);
    }

    private AuthResponse IssueTokens(UserRecord user)
    {
        var accessToken = GenerateAccessToken(user);
        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));
        var expiresAt = DateTimeOffset.UtcNow.AddDays(_refreshTokenTtlDays);
        _refreshTokens[refreshToken] = new RefreshTokenRecord(refreshToken, user, expiresAt);

        return new AuthResponse(accessToken, refreshToken, new UserProfile(user.Id, user.Email));
    }

    private string GenerateAccessToken(UserRecord user)
    {
        var handler = new JwtSecurityTokenHandler();
        var token = handler.CreateJwtSecurityToken(new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email)
            }),
            Expires = DateTime.UtcNow.AddMinutes(_accessTokenTtlMinutes),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(_jwtKey), SecurityAlgorithms.HmacSha256)
        });

        return handler.WriteToken(token);
    }

    private static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }

    private static bool VerifyPassword(string password, string storedHash)
    {
        return HashPassword(password) == storedHash;
    }

    private record UserRecord(Guid Id, string Email, string PasswordHash);
    private record RefreshTokenRecord(string Token, UserRecord User, DateTimeOffset ExpiresAt);
    private record ResetTokenRecord(string Token, string Email, DateTimeOffset ExpiresAt);
}
