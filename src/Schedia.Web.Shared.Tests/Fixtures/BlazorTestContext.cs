using System.Globalization;
using Bunit.TestDoubles;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Avemepls.Identity.DataAccess;
using FluentValidation;

using Microsoft.Extensions.Localization;

using Schedia.Auth.Domain.Validators;
using Schedia.Auth.Domain.ViaPassword;
using Schedia.Web.Shared.Services;

namespace Schedia.Web.Shared.Tests.Fixtures;

/// <summary>
/// Mock implementation of IStringLocalizer&lt;T&gt; that returns the key as the value.
/// </summary>
public class MockStringLocalizer<T> : IStringLocalizer<T>
{
    public LocalizedString this[string name] => new(name, name);
    public LocalizedString this[string name, params object[] arguments] => new(name, string.Format(name, arguments));
    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => [];
}

public class BlazorTestContext<TPage> : BlazorTestContext
    where TPage : IComponent
{
    public Mock<IStringLocalizer<TPage>> Loc { get; }

    public BlazorTestContext() : base()
    {
        Loc = new Mock<IStringLocalizer<TPage>>();

        // Configure mock to return key as value (same as MockStringLocalizer)
        Loc.Setup(l => l[It.IsAny<string>()])
            .Returns((string key) => new LocalizedString(key, key));
        Loc.Setup(l => l[It.IsAny<string>(), It.IsAny<object[]>()])
            .Returns((string key, object[] args) => new LocalizedString(key, string.Format(key, args)));

        Services.AddSingleton(Loc.Object);
    }
}

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
        // Set Russian culture for localization (avoids [brackets] wrapping in .Loc() method)
        CultureInfo.CurrentUICulture = new CultureInfo("ru-RU");

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

        // Generic mock for any IStringLocalizer<T>
        // This handles all localizers including nested types like Register.Validator
        Services.Add(new ServiceDescriptor(
            typeof(IStringLocalizer<>),
            typeof(MockStringLocalizer<>),
            ServiceLifetime.Singleton));

        // Register mock services
        Services
            .AddSingleton(MediatorMock.Object)
            .AddSingleton(AuthServiceMock.Object)
            .AddSingleton(DbContextFactoryMock.Object);

        // Real validator for RequestPasswordReset.Command used by ForgotPassword page
        // (ForgotPassword.razor casts IValidator to concrete Validator type)
        Services.AddSingleton<IValidator<RequestPasswordReset.Command>>(sp =>
            new RequestPasswordReset.Validator(
                sp.GetRequiredService<IDbContextFactory<IdentityDbContext>>(),
                new MockStringLocalizer<RequestPasswordReset.Validator>()));

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
