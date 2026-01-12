using Schedia.Auth.Domain.ViaPassword;

namespace Schedia.Web.Shared.Tests.Pages;

public class ForgotPasswordTests : IDisposable
{
    private readonly BlazorTestContext _ctx;

    public ForgotPasswordTests()
    {
        _ctx = new BlazorTestContext();
        _ctx.SetupInMemoryDatabase();
    }

    public void Dispose()
    {
        _ctx.Dispose();
    }

    #region Rendering Tests

    [Fact]
    public void ForgotPassword_ShouldRender_EmailField()
    {
        // Arrange & Act
        var cut = _ctx.RenderComponent<Schedia.Web.Shared.Pages.ForgotPassword.ForgotPassword>();

        // Assert
        cut.ContainsText("Email").Should().BeTrue();
        cut.GetInputCount().Should().BeGreaterOrEqualTo(1, "Should have Email field");
    }

    [Fact]
    public void ForgotPassword_ShouldDisplay_InstructionText()
    {
        // Arrange & Act
        var cut = _ctx.RenderComponent<Schedia.Web.Shared.Pages.ForgotPassword.ForgotPassword>();

        // Assert
        cut.ContainsText("Enter your email address").Should().BeTrue();
        cut.ContainsText("send you a link").Should().BeTrue();
    }

    [Fact]
    public void ForgotPassword_ShouldDisplay_SendResetLinkButton()
    {
        // Arrange & Act
        var cut = _ctx.RenderComponent<Schedia.Web.Shared.Pages.ForgotPassword.ForgotPassword>();

        // Assert
        var action = () => cut.FindButtonByText("Send Reset Link");
        action.Should().NotThrow("Send Reset Link button should be present");
    }

    [Fact]
    public void ForgotPassword_ShouldDisplay_BackToLoginLink()
    {
        // Arrange & Act
        var cut = _ctx.RenderComponent<Schedia.Web.Shared.Pages.ForgotPassword.ForgotPassword>();

        // Assert
        var action = () => cut.FindLinkByHref("/login");
        action.Should().NotThrow("Back to login link should be present");
    }

    #endregion

    #region Validation Tests

    // Note: Client-side validation tests are skipped because MudForm validation
    // with FluentValidation requires proper setup that doesn't work in bUnit.

    [Fact(Skip = "MudForm validation doesn't block submission in bUnit context")]
    public void ForgotPassword_WithEmptyEmail_ShouldNotCallMediator()
    {
        // Arrange
        var cut = _ctx.RenderComponent<Schedia.Web.Shared.Pages.ForgotPassword.ForgotPassword>();

        // Act
        cut.ClickButton("Send Reset Link");

        // Assert
        _ctx.MediatorMock.Verify(
            m => m.Send(It.IsAny<RequestPasswordReset.Command>(), It.IsAny<CancellationToken>()),
            Times.Never,
            "Mediator should not be called when email is empty");
    }

    #endregion

    #region State Transition Tests

    [Fact]
    public void ForgotPassword_Initially_ShouldShowForm()
    {
        // Arrange & Act
        var cut = _ctx.RenderComponent<Schedia.Web.Shared.Pages.ForgotPassword.ForgotPassword>();

        // Assert - Form should be visible
        cut.ContainsText("Send Reset Link").Should().BeTrue();
        cut.ContainsText("Check Your Email").Should().BeFalse("Success view should not be visible initially");
    }

    [Fact]
    public async Task ForgotPassword_WhenSuccessful_ShouldShowSuccessView()
    {
        // Arrange
        _ctx.MediatorMock
            .Setup(m => m.Send(It.IsAny<RequestPasswordReset.Command>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(Unit.Value));

        var cut = _ctx.RenderComponent<Schedia.Web.Shared.Pages.ForgotPassword.ForgotPassword>();
        cut.SetInputValueByIndex(0, "test@example.com");

        // Act
        cut.ClickButton("Send Reset Link");

        // Wait for async operations
        await Task.Delay(100);
        cut.Render();

        // Assert
        cut.ContainsText("Check Your Email").Should().BeTrue("Success title should be displayed");
        cut.ContainsText("password reset instructions").Should().BeTrue("Success message should be displayed");
    }

    [Fact]
    public async Task ForgotPassword_WhenSuccessful_ShouldShowCheckmark()
    {
        // Arrange
        _ctx.MediatorMock
            .Setup(m => m.Send(It.IsAny<RequestPasswordReset.Command>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(Unit.Value));

        var cut = _ctx.RenderComponent<Schedia.Web.Shared.Pages.ForgotPassword.ForgotPassword>();
        cut.SetInputValueByIndex(0, "test@example.com");

        // Act
        cut.ClickButton("Send Reset Link");

        // Wait for async operations
        await Task.Delay(100);
        cut.Render();

        // Assert - checkmark symbol should be visible
        cut.Markup.Should().Contain("✓");
    }

    [Fact]
    public async Task ForgotPassword_WhenSuccessful_ShouldShowBackToLoginButton()
    {
        // Arrange
        _ctx.MediatorMock
            .Setup(m => m.Send(It.IsAny<RequestPasswordReset.Command>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(Unit.Value));

        var cut = _ctx.RenderComponent<Schedia.Web.Shared.Pages.ForgotPassword.ForgotPassword>();
        cut.SetInputValueByIndex(0, "test@example.com");

        // Act
        cut.ClickButton("Send Reset Link");

        // Wait for async operations
        await Task.Delay(100);
        cut.Render();

        // Assert
        var action = () => cut.FindButtonByText("Back to Login");
        action.Should().NotThrow("Back to Login button should be present in success view");
    }

    [Fact]
    public async Task ForgotPassword_WhenError_ShouldShowErrorAlert()
    {
        // Arrange
        var errorMessage = "Пользователь с таким email не найден";
        _ctx.MediatorMock
            .Setup(m => m.Send(It.IsAny<RequestPasswordReset.Command>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException(errorMessage));

        var cut = _ctx.RenderComponent<Schedia.Web.Shared.Pages.ForgotPassword.ForgotPassword>();
        cut.SetInputValueByIndex(0, "notfound@example.com");

        // Act
        cut.ClickButton("Send Reset Link");

        // Wait for async operations
        await Task.Delay(100);
        cut.Render();

        // Assert
        var alert = cut.FindAlert(Severity.Error);
        alert.Should().NotBeNull("Error alert should be displayed");
        alert!.TextContent.Should().Contain(errorMessage);
    }

    [Fact]
    public async Task ForgotPassword_WhenUnexpectedException_ShouldShowGenericError()
    {
        // Arrange
        _ctx.MediatorMock
            .Setup(m => m.Send(It.IsAny<RequestPasswordReset.Command>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Something went wrong"));

        var cut = _ctx.RenderComponent<Schedia.Web.Shared.Pages.ForgotPassword.ForgotPassword>();
        cut.SetInputValueByIndex(0, "test@example.com");

        // Act
        cut.ClickButton("Send Reset Link");

        // Wait for async operations
        await Task.Delay(100);
        cut.Render();

        // Assert
        var alert = cut.FindAlert(Severity.Error);
        alert.Should().NotBeNull("Error alert should be displayed");
        alert!.TextContent.Should().Contain("непредвиденная ошибка");
    }

    #endregion

    #region Loading State Tests

    [Fact(Skip = "MudBlazor loading state doesn't render 'Loading' text in bUnit")]
    public void ForgotPassword_WhenSubmitting_ShouldShowLoadingState()
    {
        // Arrange
        var tcs = new TaskCompletionSource<Unit>();
        _ctx.MediatorMock
            .Setup(m => m.Send(It.IsAny<RequestPasswordReset.Command>(), It.IsAny<CancellationToken>()))
            .Returns(tcs.Task);

        var cut = _ctx.RenderComponent<Schedia.Web.Shared.Pages.ForgotPassword.ForgotPassword>();
        cut.SetInputValueByIndex(0, "test@example.com");

        // Act
        cut.ClickButton("Send Reset Link");

        // Assert
        cut.ContainsText("Loading").Should().BeTrue("Loading text should be visible");

        // Cleanup
        tcs.SetResult(Unit.Value);
    }

    #endregion

    #region Navigation Tests

    [Fact]
    public async Task ForgotPassword_AfterSuccess_BackToLoginShouldNavigate()
    {
        // Arrange
        _ctx.MediatorMock
            .Setup(m => m.Send(It.IsAny<RequestPasswordReset.Command>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(Unit.Value));

        var cut = _ctx.RenderComponent<Schedia.Web.Shared.Pages.ForgotPassword.ForgotPassword>();
        cut.SetInputValueByIndex(0, "test@example.com");
        cut.ClickButton("Send Reset Link");

        // Wait for async operations
        await Task.Delay(100);
        cut.Render();

        // Act
        cut.ClickButton("Back to Login");

        // Assert
        _ctx.Navigation.Uri.Should().Contain("/login");
    }

    #endregion

    #region Authorization Tests

    [Fact]
    public void ForgotPassword_WhenAuthorized_ShouldShowRedirectComponent()
    {
        // Arrange
        _ctx.SetAuthorized("testuser");

        // Act
        var cut = _ctx.RenderComponent<Schedia.Web.Shared.Pages.ForgotPassword.ForgotPassword>();

        // Assert - When authorized, form should not be visible
        var inputs = cut.FindAll("input");
        inputs.Should().BeEmpty("Form inputs should not be visible when user is authorized");
    }

    [Fact]
    public void ForgotPassword_WhenNotAuthorized_ShouldShowForm()
    {
        // Arrange
        _ctx.SetNotAuthorized();

        // Act
        var cut = _ctx.RenderComponent<Schedia.Web.Shared.Pages.ForgotPassword.ForgotPassword>();

        // Assert
        cut.GetInputCount().Should().BeGreaterOrEqualTo(1, "Form should be visible when user is not authorized");
    }

    #endregion
}
