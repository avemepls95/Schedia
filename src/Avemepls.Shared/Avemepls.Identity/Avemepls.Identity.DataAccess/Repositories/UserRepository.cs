using Avemepls.Core.DataAccess.Extensions;
using Avemepls.Core.Models;
using Avemepls.Identity.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace Avemepls.Identity.DataAccess.Repositories;

public class UserRepository(IdentityDbContext context) : IUserRepository
{
    public async Task<User?> GetByIdAsync(Id<User> id, CancellationToken ct = default)
    {
        return await context.Users
            .FirstOrDefaultAsync(u => u.Id == id && u.IsActive && u.DateDeleted == null, ct);
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default)
    {
        return await context.Users
            .FirstOrDefaultAsync(u => u.Username == username && u.IsActive && u.DateDeleted == null, ct);
    }

    public async Task<User?> FindByEmailAsync(string email, CancellationToken ct = default)
    {
        return await context.Users.Available().FirstOrDefaultAsync(u => u.Email == email, ct);
    }

    public async Task<User> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        return await context.Users.Available().FirstAsync(u => u.Email == email, ct);
    }

    public async Task<User?> GetByEmailConfirmationTokenAsync(string token, CancellationToken ct = default)
    {
        return await context.Users
            .FirstOrDefaultAsync(u => u.EmailConfirmationToken == token && u.IsActive && u.DateDeleted == null, ct);
    }

    public async Task<User?> GetByPasswordResetTokenAsync(string token, CancellationToken ct = default)
    {
        return await context.Users
            .FirstOrDefaultAsync(u => u.PasswordResetToken == token && u.IsActive && u.DateDeleted == null, ct);
    }

    public async Task<User> CreateAsync(User user, CancellationToken ct = default)
    {
        user.DateCreated = DateTimeOffset.UtcNow;
        user.IsActive = true;

        context.Users.Add(user);
        await context.SaveChangesAsync(ct);

        return user;
    }

    public async Task UpdateAsync(User user, CancellationToken ct = default)
    {
        user.DateUpdated = DateTimeOffset.UtcNow;

        context.Users.Update(user);
        await context.SaveChangesAsync(ct);
    }

    public async Task<bool> UsernameExistsAsync(string username, CancellationToken ct = default)
    {
        return await context.Users
            .AnyAsync(u => u.Username == username && u.IsActive && u.DateDeleted == null, ct);
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken ct = default)
    {
        return await context.Users
            .AnyAsync(u => u.Email == email && u.IsActive && u.DateDeleted == null, ct);
    }
}