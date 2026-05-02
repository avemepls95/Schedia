using Avemepls.Core.Extensions;
using Avemepls.Identity.DataAccess.Models;

namespace Avemepls.Auth.Domain.Extensions;

public static class UserExtensions
{
    public static string? GetFullName(this User user)
    {
        var result = string.Join(
            ' ',
            new[] { user.LastName, user.FirstName, user.Patronymic }.Where(x => !x.IsNullOrWhiteSpace()));

        return !result.IsNullOrWhiteSpace() ? result : null;
    }
}