using Microsoft.AspNetCore.Components;

namespace Avemepls.Blazor.Common;

public abstract class ServiceScopedComponentBase : OwningComponentBase
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    protected CancellationToken CancellationToken => _cancellationTokenSource.Token;

    protected override void Dispose(bool disposing)
    {
        _cancellationTokenSource.Dispose();

        base.Dispose(disposing);
    }
}