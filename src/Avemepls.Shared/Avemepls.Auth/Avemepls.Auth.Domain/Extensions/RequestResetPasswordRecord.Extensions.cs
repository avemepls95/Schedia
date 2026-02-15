using Avemepls.Identity.DataAccess.Models;

using EntityFrameworkCore.Projectables;

namespace Avemepls.Auth.Domain.Extensions;

public static class RequestResetPasswordRecordExtensions
{
    [Projectable]
    public static bool TokenIsExpired(this RequestResetPasswordRecord record, DateTimeOffset now)
        => record.TokenExpiry < now;
}