using Avemepls.Core.Models;

namespace Avemepls.Identity.DataAccess.Models;

public class ConfirmEmailRecord : IHasId
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public virtual User? User { get; set; }

    public string? EmailConfirmationToken { get; set; }

    public DateTimeOffset? EmailConfirmationTokenExpiry { get; set; }
}