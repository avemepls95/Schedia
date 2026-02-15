using Avemepls.Auth.Bearer;
using Avemepls.Auth.Domain.ViaPassword;

namespace Schedia.Web.Base.Tests.Pages;

public class RegisterTests : IDisposable
{
    private readonly BlazorTestContext<Base.Pages.Register.Register> _ctx;

    public RegisterTests()
    {
        _ctx = new BlazorTestContext<Base.Pages.Register.Register>();
        _ctx.SetupInMemoryDatabase();
    }

    public void Dispose()
    {
        _ctx.Dispose();
    }

    #region Rendering Tests

    [Fact]
    public void Register_ShouldRender_AllRequiredFields()
    {
        // Arrange & Act
        var cut = _ctx.RenderComponent<Schedia.Web.Base.Pages.Register.Register>();

        // Assert - Should have 4 fields: Username, Email, Password, ConfirmPassword
        cut.GetInputCount().Should().BeGreaterOrEqualTo(4, "Should have Username, Email, Password, and ConfirmPassword fields");
    }

    [Fact]
    public void Register_ShouldDisplay_UsernameField()
    {
        // Arrange & Act
        var cut = _ctx.RenderComponent<Schedia.Web.Base.Pages.Register.Register>();

        // Assert
        var action = () => cut.FindInputByLabel("Имя пользователя");
        action.Should().NotThrow("Username input field should be present");
    }

    [Fact]
    public void Register_ShouldDisplay_EmailField()
    {
        // Arrange & Act
        var cut = _ctx.RenderComponent<Schedia.Web.Base.Pages.Register.Register>();

        // Assert
        var action = () => cut.FindInputByLabel("Электронная почта");
        action.Should().NotThrow("Email input field should be present");
    }

    [Fact]
    public void Register_ShouldDisplay_PasswordField()
    {
        // Arrange & Act
        var cut = _ctx.RenderComponent<Schedia.Web.Base.Pages.Register.Register>();

        // Assert
        var action = () => cut.FindInputByLabel("Пароль");
        action.Should().NotThrow("Password input field should be present");
    }

    [Fact]
    public void Register_ShouldDisplay_ConfirmPasswordField()
    {
        // Arrange & Act
        var cut = _ctx.RenderComponent<Schedia.Web.Base.Pages.Register.Register>();

        // Assert
        var action = () => cut.FindInputByLabel("Подтвердите пароль");
        action.Should().NotThrow("Confirm password input field should be present");
    }

    [Fact]
    public void Register_ShouldDisplay_RegisterButton()
    {
        // Arrange & Act
        var cut = _ctx.RenderComponent<Schedia.Web.Base.Pages.Register.Register>();

        // Assert
        var action = () => cut.FindButtonByText("Регистрация");
        action.Should().NotThrow("Register button should be present");
    }

    [Fact]
    public void Register_ShouldDisplay_LoginLink()
    {
        // Arrange & Act
        var cut = _ctx.RenderComponent<Schedia.Web.Base.Pages.Register.Register>();

        // Assert
        var action = () => cut.FindLinkByHref("/login");
        action.Should().NotThrow("Login link should be present");
    }

    [Fact]
    public void Register_ShouldDisplay_AlreadyHaveAccountText()
    {
        // Arrange & Act
        var cut = _ctx.RenderComponent<Schedia.Web.Base.Pages.Register.Register>();

        // Assert
        cut.ContainsText("Уже зарегистрированы?").Should().BeTrue();
    }

    #endregion

    #region Validation Tests

    [Fact]
    public void Register_WithEmptyForm_ShouldNotCallMediator()
    {
        // Arrange
        var cut = _ctx.RenderComponent<Schedia.Web.Base.Pages.Register.Register>();

        // Act
        cut.ClickButton("Регистрация");

        // Assert
        _ctx.MediatorMock.Verify(
            m => m.Send(It.IsAny<Register.Command>(), It.IsAny<CancellationToken>()),
            Times.Never,
            "Mediator should not be called when form is invalid");
    }

    [Fact]
    public void Register_WithMismatchedPasswords_ShouldNotCallMediator()
    {
        // Arrange
        var cut = _ctx.RenderComponent<Schedia.Web.Base.Pages.Register.Register>();

        // Act - fill form with mismatched passwords
        cut.SetInputValueByIndex(0, "testuser");      // Username
        cut.SetInputValueByIndex(1, "test@email.com"); // Email
        cut.SetInputValueByIndex(2, "Password123!");   // Password
        cut.SetInputValueByIndex(3, "DifferentPass!"); // ConfirmPassword - different!
        cut.ClickButton("Регистрация");

        // Assert
        _ctx.MediatorMock.Verify(
            m => m.Send(It.IsAny<Register.Command>(), It.IsAny<CancellationToken>()),
            Times.Never,
            "Mediator should not be called when passwords don't match");
    }

    [Fact]
    public void Register_WithInvalidEmail_ShouldNotCallMediator()
    {
        // Arrange
        var cut = _ctx.RenderComponent<Schedia.Web.Base.Pages.Register.Register>();

        // Act - fill form with invalid email
        cut.SetInputValueByIndex(0, "testuser");       // Username
        cut.SetInputValueByIndex(1, "invalid-email");  // Invalid email
        cut.SetInputValueByIndex(2, "Password123!");   // Password
        cut.SetInputValueByIndex(3, "Password123!");   // ConfirmPassword
        cut.ClickButton("Регистрация");

        // Assert
        _ctx.MediatorMock.Verify(
            m => m.Send(It.IsAny<Register.Command>(), It.IsAny<CancellationToken>()),
            Times.Never,
            "Mediator should not be called when email is invalid");
    }

    #endregion

    #region State Tests

    [Fact(Skip = "MudBlazor loading indicator doesn't render expected elements in bUnit")]
    public void Register_WhenSubmitting_ShouldShowLoadingState()
    {
        // Arrange
        var tcs = new TaskCompletionSource<TokenInformation>();
        _ctx.MediatorMock
            .Setup(m => m.Send(It.IsAny<Register.Command>(), It.IsAny<CancellationToken>()))
            .Returns(tcs.Task);

        var cut = _ctx.RenderComponent<Schedia.Web.Base.Pages.Register.Register>();
        cut.SetInputValueByIndex(0, "newuser");
        cut.SetInputValueByIndex(1, "new@example.com");
        cut.SetInputValueByIndex(2, "Password123!");
        cut.SetInputValueByIndex(3, "Password123!");

        // Act
        cut.ClickButton("Регистрация");

        // Assert
        cut.HasLoadingIndicator().Should().BeTrue("Loading indicator should be visible during submission");

        // Cleanup
        tcs.SetResult(new TokenInformation { AccessToken = "token", RefreshToken = "refresh" });
    }

    [Fact]
    public async Task Register_WhenSuccessful_ShouldCallAuthService()
    {
        // Arrange
        var tokenInfo = new TokenInformation
        {
            AccessToken = "test-token",
            RefreshToken = "refresh-token",
        };

        _ctx.MediatorMock
            .Setup(m => m.Send(It.IsAny<Register.Command>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(tokenInfo);

        var cut = _ctx.RenderComponent<Schedia.Web.Base.Pages.Register.Register>();
        cut.SetInputValueByIndex(0, "newuser");
        cut.SetInputValueByIndex(1, "new@example.com");
        cut.SetInputValueByIndex(2, "Password123!");
        cut.SetInputValueByIndex(3, "Password123!");

        // Act
        cut.ClickButton("Регистрация");

        // Wait for async operations
        await Task.Delay(100);

        // Assert
        _ctx.AuthServiceMock.Verify(
            s => s.LoginAsync(It.Is<TokenInformation>(t => t.AccessToken == "test-token"), It.IsAny<string>()),
            Times.Once,
            "AuthService.LoginAsync should be called after successful registration");
    }

    [Fact]
    public async Task Register_WhenException_ShouldShowError()
    {
        // Arrange
        _ctx.MediatorMock
            .Setup(m => m.Send(It.IsAny<Register.Command>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Registration failed"));

        var cut = _ctx.RenderComponent<Schedia.Web.Base.Pages.Register.Register>();
        cut.SetInputValueByIndex(0, "newuser");
        cut.SetInputValueByIndex(1, "new@example.com");
        cut.SetInputValueByIndex(2, "Password123!");
        cut.SetInputValueByIndex(3, "Password123!");

        // Act
        cut.ClickButton("Регистрация");

        // Wait for async operations
        await Task.Delay(100);
        cut.Render();

        // Assert
        var alert = cut.FindAlert(Severity.Error);
        alert.Should().NotBeNull("Error alert should be displayed on exception");
    }

    #endregion

    #region Authorization Tests

    [Fact]
    public void Register_WhenAuthorized_ShouldShowRedirectComponent()
    {
        // Arrange
        _ctx.SetAuthorized("testuser");

        // Act
        var cut = _ctx.RenderComponent<Schedia.Web.Base.Pages.Register.Register>();

        // Assert - When authorized, form should not be visible
        var inputs = cut.FindAll("input");
        inputs.Should().BeEmpty("Form inputs should not be visible when user is authorized");
    }

    [Fact]
    public void Register_WhenNotAuthorized_ShouldShowRegistrationForm()
    {
        // Arrange
        _ctx.SetNotAuthorized();

        // Act
        var cut = _ctx.RenderComponent<Schedia.Web.Base.Pages.Register.Register>();

        // Assert
        cut.GetInputCount().Should().BeGreaterOrEqualTo(4, "Registration form should be visible when user is not authorized");
    }

    #endregion
}
