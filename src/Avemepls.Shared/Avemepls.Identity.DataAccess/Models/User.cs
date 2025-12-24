using Avemepls.Core.Models;

namespace Avemepls.Identity.DataAccess.Models;

public readonly record struct UserId(int Value) : IHasValue<int>;

public class User : IHasId<UserId, int>
{
    public UserId Id { get; set; }

    public string Username { get; set; }

    public string? Email { get; set; }

    public string PasswordHash { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTimeOffset DateCreated { get; set; }

    public DateTimeOffset? DateUpdated { get; set; }

    public DateTimeOffset? DateDeleted { get; set; }
}