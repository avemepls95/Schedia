using Avemepls.Core.Models;

namespace Avemepls.Identity.DataAccess.Models;

public class User : IHasId<int>, IHasDateCreated, IHasDateUpdated, IHasIsActive, IHasDateDeleted
{
    public int Id { get; set; }

    public string Username { get; set; }

    public string? Email { get; set; }

    public string PasswordHash { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTimeOffset DateCreated { get; set; }

    public DateTimeOffset? DateUpdated { get; set; }

    public DateTimeOffset? DateDeleted { get; set; }

    public bool EmailConfirmed { get; set; }
}