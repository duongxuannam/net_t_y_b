namespace NetTYB.Api.Models;

public record AuthRequest(string Email, string Password);
public record RefreshRequest(string RefreshToken);
public record ForgotPasswordRequest(string Email);
public record ResetPasswordRequest(string Email, string Token, string NewPassword);

public record AuthResponse(string AccessToken, string RefreshToken, UserProfile User);
public record UserProfile(Guid Id, string Email);
