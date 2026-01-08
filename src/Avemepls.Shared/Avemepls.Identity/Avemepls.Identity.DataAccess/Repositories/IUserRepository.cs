using Avemepls.Core.Models;
using Avemepls.Identity.DataAccess.Models;

namespace Avemepls.Identity.DataAccess.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Id<User> id, CancellationToken ct = default);

    Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default);

    Task<User?> FindByEmailAsync(string email, CancellationToken ct = default);
    Task<User> GetByEmailAsync(string email, CancellationToken ct = default);

    Task<User?> GetByEmailConfirmationTokenAsync(string token, CancellationToken ct = default);

    Task<User?> GetByPasswordResetTokenAsync(string token, CancellationToken ct = default);

    Task<User> CreateAsync(User user, CancellationToken ct = default);

    Task UpdateAsync(User user, CancellationToken ct = default);

    Task<bool> UsernameExistsAsync(string username, CancellationToken ct = default);

    Task<bool> EmailExistsAsync(string email, CancellationToken ct = default);
}