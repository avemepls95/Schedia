using Avemepls.Core.DataAccess.ContextInitializing;

using Microsoft.EntityFrameworkCore;

namespace Avemepls.Identity.DataAccess;

internal sealed class ContextInitializer(IdentityDbContext dbContext) : IContextInitializer
{
    private bool _disposed;

    public void Initialize()
    {
        dbContext.Database.Migrate();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        dbContext.Dispose();
        _disposed = true;
    }
}