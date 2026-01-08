using Avemepls.Auth.Password.Abstractions;
using Avemepls.Auth.Password.Models;

using Microsoft.AspNetCore.Mvc;

namespace Schedia.Web.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var user = await _authService.Authenticate(new AuthenticateRequest { Password = request.Password, Username = request.Username });

            if (user == null)
            {
                return Unauthorized(new { message = "Invalid username or password" });
            }

            return Ok(new
            {
                success = true,
                username = user.Username,
                emailConfirmed = user.EmailConfirmed
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login failed for username: {Username}", request.Username);
            return BadRequest(new { message = "Login failed" });
        }
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            if (request.Password != request.ConfirmPassword)
            {
                return BadRequest(new { message = "Passwords do not match" });
            }

            var user = await _authService.Register(new Avemepls.Auth.Password.Models.RegisterRequest
            {
                Username = request.Username,
                Email = request.Email,
                Password = request.Password
            });

            return Ok(new
            {
                success = true,
                message = "Registration successful. Please check your email to confirm your address.",
                username = user.Username
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Registration failed");
            return BadRequest(new { message = "Registration failed" });
        }
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _authService.LogoutAsync();
        return Ok(new { success = true });
    }

    [HttpPost("password-reset/request")]
    public async Task<IActionResult> RequestPasswordReset([FromBody] PasswordResetRequest request)
    {
        await _authService.RequestPasswordResetAsync(new RequestPasswordResetRequest { Email = request.Email });

        return Ok(new { message = "If the email exists, a password reset link has been sent." });
    }

    [HttpPost("password-reset/confirm")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var success = await _authService.ResetPasswordAsync(request.Token, request.NewPassword);

        if (!success)
        {
            return BadRequest(new { message = "Invalid or expired reset token" });
        }

        return Ok(new { success = true, message = "Password reset successful" });
    }

    [HttpGet("email-confirmation")]
    public async Task<IActionResult> ConfirmEmail([FromQuery] string token)
    {
        var success = await _authService.ConfirmEmailAsync(token);

        if (!success)
        {
            return BadRequest(new { message = "Invalid or expired confirmation token" });
        }

        return Ok(new { success = true, message = "Email confirmed successfully" });
    }

    [HttpPost("email-confirmation/resend")]
    public async Task<IActionResult> ResendEmailConfirmation([FromBody] ResendEmailRequest request)
    {
        await _authService.ResendEmailConfirmationAsync(request.Email);

        return Ok(new { message = "If the email exists, a new confirmation link has been sent." });
    }
}

public record LoginRequest(string Username, string Password);
public record RegisterRequest(string Username, string Email, string Password, string ConfirmPassword);
public record PasswordResetRequest(string Email);
public record ResetPasswordRequest(string Token, string NewPassword);
public record ResendEmailRequest(string Email);