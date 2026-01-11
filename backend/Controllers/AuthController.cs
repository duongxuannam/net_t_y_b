using Microsoft.AspNetCore.Mvc;
using NetTYB.Api.Models;
using NetTYB.Api.Services;

namespace NetTYB.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly AuthCookieOptions _cookieOptions;
    private readonly PasswordResetOptions _passwordResetOptions;

    public AuthController(
        AuthService authService,
        AuthCookieOptions cookieOptions,
        PasswordResetOptions passwordResetOptions)
    {
        _authService = authService;
        _cookieOptions = cookieOptions;
        _passwordResetOptions = passwordResetOptions;
    }

    [HttpPost("register")]
    public ActionResult<AuthResponse> Register(AuthRequest request)
    {
        try
        {
            var response = _authService.Register(request.Email, request.Password);
            SetRefreshCookie(response.RefreshToken);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("login")]
    public ActionResult<AuthResponse> Login(AuthRequest request)
    {
        try
        {
            var response = _authService.Login(request.Email, request.Password);
            SetRefreshCookie(response.RefreshToken);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpPost("refresh")]
    public ActionResult<AuthResponse> Refresh(RefreshRequest request)
    {
        try
        {
            var token = string.IsNullOrWhiteSpace(request.RefreshToken)
                ? Request.Cookies[_cookieOptions.RefreshCookieName] ?? string.Empty
                : request.RefreshToken;
            var response = _authService.Refresh(token);
            SetRefreshCookie(response.RefreshToken);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpPost("logout")]
    public IActionResult Logout(RefreshRequest request)
    {
        var token = string.IsNullOrWhiteSpace(request.RefreshToken)
            ? Request.Cookies[_cookieOptions.RefreshCookieName] ?? string.Empty
            : request.RefreshToken;
        if (!string.IsNullOrWhiteSpace(token))
        {
            _authService.Logout(token);
        }
        Response.Cookies.Delete(_cookieOptions.RefreshCookieName);
        return NoContent();
    }

    [HttpPost("forgot")]
    public IActionResult Forgot(ForgotPasswordRequest request)
    {
        try
        {
            var token = _authService.GeneratePasswordReset(request.Email);
            var resetUrl = $"{_passwordResetOptions.BaseUrl}?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(request.Email)}";
            return Ok(new { message = "Password reset token generated.", token, resetUrl });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost("reset")]
    public IActionResult Reset(ResetPasswordRequest request)
    {
        try
        {
            _authService.ResetPassword(request.Email, request.Token, request.NewPassword);
            return Ok(new { message = "Password updated." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private void SetRefreshCookie(string refreshToken)
    {
        Response.Cookies.Append(_cookieOptions.RefreshCookieName, refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = _cookieOptions.RefreshCookieSecure,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddDays(_cookieOptions.RefreshTokenTtlDays)
        });
    }
}
