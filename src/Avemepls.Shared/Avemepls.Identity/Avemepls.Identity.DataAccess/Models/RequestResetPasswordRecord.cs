using Avemepls.Core.Models;

namespace Avemepls.Identity.DataAccess.Models;

public class RequestResetPasswordRecord : IHasId
{
    public int Id { get; set; }
    public int UserId { get; set; }

    public virtual User? User { get; set; }

    public string? Token { get; set; }

    public DateTimeOffset? TokenExpiry { get; set; }
}