using Avemepls.Auth.Password.Models;
using Avemepls.Core.Models;
using Avemepls.Identity.DataAccess.Models;

namespace Avemepls.Auth.Password.Abstractions;

public interface IAuthService
{
    Task<User?> Authenticate(AuthenticateRequest request, CancellationToken cancellationToken = default);

    Task<User> Register(RegisterRequest registerRequest, CancellationToken cancellationToken = default);

    Task<bool> ChangePasswordAsync(Id<User> userId, string oldPassword, string newPassword, CancellationToken cancellationToken = default);

    Task LogoutAsync();

    Task<bool> ConfirmEmailAsync(string token, CancellationToken cancellationToken = default);

    Task ResendEmailConfirmationAsync(string email, CancellationToken cancellationToken = default);

    Task RequestPasswordResetAsync(RequestPasswordResetRequest request, CancellationToken cancellationToken = default);

    Task<bool> ResetPasswordAsync(string token, string newPassword, CancellationToken cancellationToken = default);

    Task<User?> GetCurrentUserAsync();
}