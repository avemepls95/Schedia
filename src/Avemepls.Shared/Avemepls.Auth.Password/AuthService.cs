using System.Security.Claims;
using System.Security.Cryptography;

using Avemepls.Auth.Password.Abstractions;
using Avemepls.Auth.Password.Models;
using Avemepls.Core.Extensions;
using Avemepls.Core.Models;
using Avemepls.Identity.DataAccess.Models;
using Avemepls.Identity.DataAccess.Repositories;
using Avemepls.Infrastructure.Email;

using FluentValidation;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Avemepls.Auth.Password;

public class AuthService(
    IUserRepository userRepository,
    IEmailService emailService,
    IHttpContextAccessor httpContextAccessor,
    IConfiguration configuration,
    IServiceProvider serviceProvider)
    : IAuthService
{
    private readonly string _baseUrl = configuration["AppSettings:BaseUrl"];

    public async Task<User?> Authenticate(AuthenticateRequest request, CancellationToken cancellationToken = default)
    {
        var validator = serviceProvider.GetRequiredService<IValidator<AuthenticateRequest>>();
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var user = await userRepository.GetByUsernameAsync(request.Username, cancellationToken);

        if (user == null || !PasswordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            return null;
        }

        await AuthenticateUser(user);

        return user;
    }

    public async Task<User> Register(RegisterRequest registerRequest, CancellationToken cancellationToken = default)
    {
        var validator = serviceProvider.GetRequiredService<IValidator<RegisterRequest>>();
        await validator.ValidateAndThrowAsync(registerRequest, cancellationToken);

        var confirmationToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

        var username = registerRequest.Username.IsNullOrWhiteSpace()
            ? registerRequest.Email
            : registerRequest.Username!;

        var user = new User
        {
            Username = username,
            Email = registerRequest.Email,
            PasswordHash = PasswordHasher.HashPassword(registerRequest.Password),
            IsActive = true,
            EmailConfirmed = false,
            EmailConfirmationToken = confirmationToken,
            EmailConfirmationTokenExpiry = DateTimeOffset.UtcNow.AddDays(7),
            DateCreated = DateTimeOffset.UtcNow
        };

        var created = await userRepository.CreateAsync(user, cancellationToken);

        // TODO: via INotificationHandler
        var confirmationLink = $"{_baseUrl}/confirm-email?token={Uri.EscapeDataString(confirmationToken)}";
        await emailService.SendEmailConfirmationAsync(registerRequest.Email, username, confirmationLink, cancellationToken);

        await AuthenticateUser(created);

        return created;
    }

    public async Task<bool> ChangePasswordAsync(Id<User> userId, string oldPassword, string newPassword, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(userId, cancellationToken);

        if (user == null || !PasswordHasher.VerifyPassword(oldPassword, user.PasswordHash))
        {
            return false;
        }

        user.PasswordHash = PasswordHasher.HashPassword(newPassword);
        user.DateUpdated = DateTimeOffset.UtcNow;

        await userRepository.UpdateAsync(user, cancellationToken);

        return true;
    }

    public async Task LogoutAsync()
    {
        await httpContextAccessor.HttpContext!.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }

    public async Task<bool> ConfirmEmailAsync(string token, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByEmailConfirmationTokenAsync(token, cancellationToken);

        if (user == null || user.EmailConfirmationTokenExpiry < DateTimeOffset.UtcNow)
        {
            return false;
        }

        user.EmailConfirmed = true;
        user.EmailConfirmationToken = null;
        user.EmailConfirmationTokenExpiry = null;
        user.DateUpdated = DateTimeOffset.UtcNow;

        await userRepository.UpdateAsync(user, cancellationToken);

        return true;
    }

    public async Task ResendEmailConfirmationAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.FindByEmailAsync(email, cancellationToken);

        if (user == null || user.EmailConfirmed)
        {
            return;
        }

        var confirmationToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

        user.EmailConfirmationToken = confirmationToken;
        user.EmailConfirmationTokenExpiry = DateTimeOffset.UtcNow.AddDays(7);
        user.DateUpdated = DateTimeOffset.UtcNow;

        await userRepository.UpdateAsync(user, cancellationToken);

        var confirmationLink = $"{_baseUrl}/confirm-email?token={Uri.EscapeDataString(confirmationToken)}";
        await emailService.SendEmailConfirmationAsync(email, user.Username, confirmationLink, cancellationToken);
    }

    public async Task RequestPasswordResetAsync(RequestPasswordResetRequest request, CancellationToken cancellationToken = default)
    {
        var validator = serviceProvider.GetRequiredService<IValidator<RequestPasswordResetRequest>>();
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var user = await userRepository.GetByEmailAsync(request.Email, cancellationToken);

        var resetToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

        user.PasswordResetToken = resetToken;
        user.PasswordResetTokenExpiry = DateTimeOffset.UtcNow.AddHours(1);
        user.DateUpdated = DateTimeOffset.UtcNow;

        await userRepository.UpdateAsync(user, cancellationToken);

        var resetLink = $"{_baseUrl}/reset-password?token={Uri.EscapeDataString(resetToken)}";
        await emailService.SendPasswordResetAsync(request.Email, user.Username, resetLink, cancellationToken);
    }

    public async Task<bool> ResetPasswordAsync(string token, string newPassword, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByPasswordResetTokenAsync(token, cancellationToken);

        if (user == null || user.PasswordResetTokenExpiry < DateTimeOffset.UtcNow)
        {
            return false;
        }

        user.PasswordHash = PasswordHasher.HashPassword(newPassword);
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiry = null;
        user.DateUpdated = DateTimeOffset.UtcNow;

        await userRepository.UpdateAsync(user, cancellationToken);

        return true;
    }

    public async Task<User?> GetCurrentUserAsync()
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext?.User?.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
        {
            return null;
        }

        return await userRepository.GetByIdAsync(new Id<User>(userId));
    }

    private async Task AuthenticateUser(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Email, user.Email ?? "")
        };

        if (user.EmailConfirmed)
        {
            claims.Add(new Claim("EmailConfirmed", "true"));
        }

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30)
        };

        await httpContextAccessor.HttpContext!.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);
    }
}