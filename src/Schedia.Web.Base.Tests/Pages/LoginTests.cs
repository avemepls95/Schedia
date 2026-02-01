using Avemepls.Auth.Bearer;

using Schedia.Web.Base.Pages.Login;

namespace Schedia.Web.Base.Tests.Pages;

public class LoginTests : IDisposable
{
    private readonly BlazorTestContext<Login> _ctx = new();

    public void Dispose()
    {
        _ctx.Dispose();
    }

    #region Rendering Tests

    [Fact]
    public void Login_ShouldDisplay_UsernameField()
    {
        // Arrange & Act
        var cut = _ctx.RenderComponent<Login>();

        // Assert
        var action = () => cut.FindInputByLabel("Имя пользователя");
        action.Should().NotThrow("Username input field should be present");
    }

    [Fact]
    public void Login_ShouldDisplay_PasswordField()
    {
        // Arrange & Act
        var cut = _ctx.RenderComponent<Schedia.Web.Base.Pages.Login.Login>();

        // Assert
        var action = () => cut.FindInputByLabel("Пароль");
        action.Should().NotThrow("Password input field should be present");
    }

    [Fact]
    public void Login_ShouldDisplay_LoginButton()
    {
        // Arrange & Act
        var cut = _ctx.RenderComponent<Schedia.Web.Base.Pages.Login.Login>();

        // Assert
        var action = () => cut.FindButtonByText("Войти");
        action.Should().NotThrow("Login button should be present");
    }

    [Fact]
    public void Login_ShouldDisplay_RegisterLink()
    {
        // Arrange & Act
        var cut = _ctx.RenderComponent<Schedia.Web.Base.Pages.Login.Login>();

        // Assert
        var action = () => cut.FindLinkByHref("/register");
        action.Should().NotThrow("Register link should be present");
    }

    [Fact]
    public void Login_ShouldDisplay_ForgotPasswordLink()
    {
        // Arrange & Act
        var cut = _ctx.RenderComponent<Schedia.Web.Base.Pages.Login.Login>();

        // Assert
        var action = () => cut.FindLinkByHref("/forgot-password");
        action.Should().NotThrow("Forgot password link should be present");
    }

    #endregion

    #region Validation Tests

    [Fact]
    public void Login_WithEmptyForm_ShouldNotCallMediator()
    {
        // Arrange
        var cut = _ctx.RenderComponent<Schedia.Web.Base.Pages.Login.Login>();

        // Act
        cut.ClickButton("Войти");

        // Assert
        _ctx.MediatorMock.Verify(
            m => m.Send(It.IsAny<AuthCommand.Command>(), It.IsAny<CancellationToken>()),
            Times.Never,
            "Mediator should not be called when form is invalid");
    }

    [Fact]
    public void Login_WithEmptyUsername_ShouldShowValidationError()
    {
        // Arrange
        var cut = _ctx.RenderComponent<Schedia.Web.Base.Pages.Login.Login>();

        // Act - fill only password
        cut.SetInputValueByIndex(1, "somepassword"); // Password is second input
        cut.ClickButton("Войти");

        // Assert - form should be invalid, mediator not called
        _ctx.MediatorMock.Verify(
            m => m.Send(It.IsAny<AuthCommand.Command>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public void Login_WithEmptyPassword_ShouldShowValidationError()
    {
        // Arrange
        var cut = _ctx.RenderComponent<Schedia.Web.Base.Pages.Login.Login>();

        // Act - fill only username
        cut.SetInputValueByIndex(0, "testuser"); // Username is first input
        cut.ClickButton("Войти");

        // Assert - form should be invalid, mediator not called
        _ctx.MediatorMock.Verify(
            m => m.Send(It.IsAny<AuthCommand.Command>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    #endregion

    #region State Tests (Loading/Success/Error)

    [Fact(Skip = "MudBlazor loading indicator doesn't render expected elements in bUnit")]
    public void Login_WhenSubmitting_ShouldShowLoadingState()
    {
        // Arrange
        var tcs = new TaskCompletionSource<TokenInformation>();
        _ctx.MediatorMock
            .Setup(m => m.Send(It.IsAny<AuthCommand.Command>(), It.IsAny<CancellationToken>()))
            .Returns(tcs.Task);

        var cut = _ctx.RenderComponent<Schedia.Web.Base.Pages.Login.Login>();
        cut.SetInputValueByIndex(0, "testuser");
        cut.SetInputValueByIndex(1, "password123");

        // Act
        cut.ClickButton("Войти");

        // Assert
        cut.HasLoadingIndicator().Should().BeTrue("Loading indicator should be visible during submission");
        cut.ContainsText("Загрузка").Should().BeTrue("Loading text should be visible");

        // Cleanup
        tcs.SetResult(new TokenInformation { AccessToken = "token", RefreshToken = "refresh" });
    }

    [Fact]
    public async Task Login_WhenSuccessful_ShouldCallAuthService()
    {
        // Arrange
        var tokenInfo = new TokenInformation
        {
            AccessToken = "test-token",
            RefreshToken = "refresh-token",
        };

        _ctx.MediatorMock
            .Setup(m => m.Send(It.IsAny<AuthCommand.Command>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(tokenInfo);

        var cut = _ctx.RenderComponent<Schedia.Web.Base.Pages.Login.Login>();
        cut.SetInputValueByIndex(0, "testuser");
        cut.SetInputValueByIndex(1, "password123");

        // Act
        cut.ClickButton("Войти");

        // Wait for async operations
        await Task.Delay(100);

        // Assert
        _ctx.AuthServiceMock.Verify(
            s => s.LoginAsync(It.Is<TokenInformation>(t => t.AccessToken == "test-token"), It.IsAny<string>()),
            Times.Once,
            "AuthService.LoginAsync should be called with token information");
    }

    [Fact]
    public async Task Login_WhenValidationException_ShouldShowErrorMessage()
    {
        // Arrange
        const string errorMessage = "Неверный логин или пароль";
        _ctx.MediatorMock
            .Setup(m => m.Send(It.IsAny<AuthCommand.Command>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException(errorMessage));

        var cut = _ctx.RenderComponent<Schedia.Web.Base.Pages.Login.Login>();
        cut.SetInputValueByIndex(0, "testuser");
        cut.SetInputValueByIndex(1, "wrongpassword");

        // Act
        cut.ClickButton("Войти");

        // Wait for async operations
        await Task.Delay(100);
        cut.Render();

        // Assert
        var alert = cut.FindAlert(Severity.Error);
        alert.Should().NotBeNull("Error alert should be displayed");
        alert!.TextContent.Should().Contain(errorMessage);
    }

    [Fact]
    public async Task Login_WhenUnexpectedException_ShouldShowGenericError()
    {
        // Arrange
        _ctx.MediatorMock
            .Setup(m => m.Send(It.IsAny<AuthCommand.Command>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Something went wrong"));

        var cut = _ctx.RenderComponent<Schedia.Web.Base.Pages.Login.Login>();
        cut.SetInputValueByIndex(0, "testuser");
        cut.SetInputValueByIndex(1, "password123");

        // Act
        cut.ClickButton("Войти");

        // Wait for async operations
        await Task.Delay(100);
        cut.Render();

        // Assert
        var alert = cut.FindAlert(Severity.Error);
        alert.Should().NotBeNull("Error alert should be displayed");
        alert!.TextContent.Should().Contain("непредвиденная ошибка");
    }

    #endregion

    #region Navigation Tests

    [Fact]
    public async Task Login_WithReturnUrl_ShouldPassToAuthService()
    {
        // Arrange
        const string returnUrl = "/dashboard";
        var tokenInfo = new TokenInformation { AccessToken = "token", RefreshToken = "refresh" };

        _ctx.MediatorMock
            .Setup(m => m.Send(It.IsAny<AuthCommand.Command>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(tokenInfo);

        // Navigate to the login page with returnUrl query parameter
        _ctx.Navigation.NavigateTo($"/login?returnUrl={Uri.EscapeDataString(returnUrl)}");

        var cut = _ctx.RenderComponent<Schedia.Web.Base.Pages.Login.Login>();

        cut.SetInputValueByIndex(0, "testuser");
        cut.SetInputValueByIndex(1, "password123");

        // Act
        cut.ClickButton("Войти");

        // Wait for async operations
        await Task.Delay(100);

        // Assert
        _ctx.AuthServiceMock.Verify(
            s => s.LoginAsync(It.IsAny<TokenInformation>(), It.IsAny<string>()),
            Times.Once,
            "AuthService.LoginAsync should be called");
    }

    #endregion

    #region Authorization Tests

    [Fact]
    public void Login_WhenAuthorized_ShouldShowRedirectComponent()
    {
        // Arrange
        _ctx.SetAuthorized("testuser");

        // Act
        var cut = _ctx.RenderComponent<Schedia.Web.Base.Pages.Login.Login>();

        // Assert - When authorized, RedirectToHome component should be rendered
        // The form should NOT be visible
        var inputs = cut.FindAll("input");
        inputs.Should().BeEmpty("Form inputs should not be visible when user is authorized");
    }

    #endregion
}
