using Avemepls.Core.Models;
using Avemepls.Identity.DataAccess.Models;

namespace Avemepls.Auth.Password.Abstractions;

public interface IAuthService
{
    Task<User?> AuthenticateAsync(string username, string password);

    Task<User> RegisterAsync(string username, string email, string password);

    Task<bool> ChangePasswordAsync(Id<User> userId, string oldPassword, string newPassword);

    Task LogoutAsync();
}