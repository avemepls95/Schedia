using Bunit.TestDoubles;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Avemepls.Identity.DataAccess;
using Schedia.Web.Shared.Services;

namespace Schedia.Web.Shared.Tests.Fixtures;

/// <summary>
/// Base test context with MudBlazor services and common mocks configured.
/// </summary>
public class BlazorTestContext : TestContext
{
    private readonly TestAuthorizationContext? _authContext;

    public Mock<IMediator> MediatorMock { get; }
    public Mock<IAuthenticationService> AuthServiceMock { get; }
    public Mock<IDbContextFactory<IdentityDbContext>> DbContextFactoryMock { get; }
    public FakeNavigationManager Navigation => Services.GetRequiredService<FakeNavigationManager>();

    public BlazorTestContext()
    {
        // Initialize mocks
        MediatorMock = new Mock<IMediator>();
        AuthServiceMock = new Mock<IAuthenticationService>();
        DbContextFactoryMock = new Mock<IDbContextFactory<IdentityDbContext>>();

        // Configure MudBlazor services
        Services.AddMudServices(options =>
        {
            options.SnackbarConfiguration.ShowTransitionDuration = 0;
            options.SnackbarConfiguration.HideTransitionDuration = 0;
        });

        // Setup JSInterop for MudBlazor (loose mode allows any JS call)
        JSInterop.Mode = JSRuntimeMode.Loose;

        // Setup specific MudBlazor JS interop calls
        SetupMudBlazorJsInterop();

        // Register mock services
        Services.AddSingleton(MediatorMock.Object);
        Services.AddSingleton(AuthServiceMock.Object);
        Services.AddSingleton(DbContextFactoryMock.Object);

        // Setup default authorization state (not authorized)
        _authContext = this.AddTestAuthorization();
        _authContext.SetNotAuthorized();
    }

    private void SetupMudBlazorJsInterop()
    {
        // Setup common MudBlazor JS interop calls
        JSInterop.SetupVoid("mudPopover.initialize", _ => true);
        JSInterop.SetupVoid("mudKeyInterceptor.connect", _ => true);
        JSInterop.SetupVoid("mudElementReference.saveFocus", _ => true);
        JSInterop.Setup<int>("mudElementReference.addDefaultPreventingHandler", _ => true);
        JSInterop.Setup<bool>("mudScrollManager.lockScroll", _ => true);
    }

    /// <summary>
    /// Sets up the authorization state to authorized.
    /// </summary>
    public void SetAuthorized(string userName = "testuser")
    {
        _authContext?.SetAuthorized(userName);
    }

    /// <summary>
    /// Sets up the authorization state to not authorized.
    /// </summary>
    public void SetNotAuthorized()
    {
        _authContext?.SetNotAuthorized();
    }

    /// <summary>
    /// Navigates to a page with query parameters.
    /// </summary>
    public void NavigateToWithQuery(string basePath, Dictionary<string, string?> queryParams)
    {
        var queryString = string.Join("&", queryParams
            .Where(p => p.Value != null)
            .Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value!)}"));
        var uri = string.IsNullOrEmpty(queryString) ? basePath : $"{basePath}?{queryString}";
        Navigation.NavigateTo(uri);
    }

    /// <summary>
    /// Creates a mock DbContext for in-memory database testing.
    /// </summary>
    public void SetupInMemoryDatabase()
    {
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        DbContextFactoryMock
            .Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new IdentityDbContext(options));

        DbContextFactoryMock
            .Setup(f => f.CreateDbContext())
            .Returns(() => new IdentityDbContext(options));
    }
}
