using Schedia.Identity.Domain.User;

namespace Schedia.Web.Shared.Tests.Pages;

public class ConfirmEmailTests : IDisposable
{
    private readonly BlazorTestContext<Shared.Pages.ConfirmEmail.ConfirmEmail> _ctx = new();

    public void Dispose()
    {
        _ctx.Dispose();
    }

    private void NavigateToConfirmEmail(string? token)
    {
        var uri = token != null
            ? $"/confirm-email?token={Uri.EscapeDataString(token)}"
            : "/confirm-email";
        _ctx.Navigation.NavigateTo(uri);
    }

    #region Initial State Tests

    [Fact]
    public void ConfirmEmail_Initially_ShouldShowLoadingState()
    {
        // Arrange
        var tcs = new TaskCompletionSource<Unit>();
        _ctx.MediatorMock
            .Setup(m => m.Send(It.IsAny<ConfirmEmail.Command>(), It.IsAny<CancellationToken>()))
            .Returns(tcs.Task);

        NavigateToConfirmEmail("valid-token");

        // Act
        var cut = _ctx.RenderComponent<Schedia.Web.Shared.Pages.ConfirmEmail.ConfirmEmail>();

        // Assert
        cut.ContainsText("Подтверждение Вашего электронного адреса").Should().BeTrue("Loading message should be visible initially");
        cut.Markup.Should().Contain("⏳", "Hourglass emoji should be visible during loading");

        // Cleanup
        tcs.SetResult(Unit.Value);
    }

    #endregion

    #region Success State Tests

    [Fact]
    public async Task ConfirmEmail_WhenSuccessful_ShouldShowSuccessView()
    {
        // Arrange
        _ctx.MediatorMock
            .Setup(m => m.Send(It.IsAny<ConfirmEmail.Command>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(Unit.Value));

        NavigateToConfirmEmail("valid-token");

        // Act
        var cut = _ctx.RenderComponent<Schedia.Web.Shared.Pages.ConfirmEmail.ConfirmEmail>();

        // Wait for OnAfterRenderAsync to complete
        await Task.Delay(200);
        cut.Render();

        // Assert
        cut.ContainsText("Электронный адрес подтвержден!").Should().BeTrue("Success title should be displayed");
        cut.ContainsText("Ваш e-mail успешно подтвержден").Should().BeTrue("Success message should be displayed");
    }

    [Fact]
    public async Task ConfirmEmail_WhenSuccessful_ShouldShowCheckmark()
    {
        // Arrange
        _ctx.MediatorMock
            .Setup(m => m.Send(It.IsAny<ConfirmEmail.Command>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(Unit.Value));

        NavigateToConfirmEmail("valid-token");

        // Act
        var cut = _ctx.RenderComponent<Schedia.Web.Shared.Pages.ConfirmEmail.ConfirmEmail>();

        // Wait for OnAfterRenderAsync to complete
        await Task.Delay(200);
        cut.Render();

        // Assert
        cut.Markup.Should().Contain("✓", "Checkmark should be visible on success");
    }

    [Fact]
    public async Task ConfirmEmail_WhenSuccessful_ShouldShowGoToHomeButton()
    {
        // Arrange
        _ctx.MediatorMock
            .Setup(m => m.Send(It.IsAny<ConfirmEmail.Command>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(Unit.Value));

        NavigateToConfirmEmail("valid-token");

        // Act
        var cut = _ctx.RenderComponent<Schedia.Web.Shared.Pages.ConfirmEmail.ConfirmEmail>();

        // Wait for OnAfterRenderAsync to complete
        await Task.Delay(200);
        cut.Render();

        // Assert
        cut.ContainsText("На главную").Should().BeTrue("Go to Home button should be present");
    }

    [Fact]
    public async Task ConfirmEmail_WhenSuccessful_ShouldCallMediator()
    {
        // Arrange
        _ctx.MediatorMock
            .Setup(m => m.Send(It.IsAny<ConfirmEmail.Command>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(Unit.Value));

        NavigateToConfirmEmail("valid-token");

        // Act
        var cut = _ctx.RenderComponent<Schedia.Web.Shared.Pages.ConfirmEmail.ConfirmEmail>();

        // Wait for OnAfterRenderAsync to complete
        await Task.Delay(200);

        // Assert
        _ctx.MediatorMock.Verify(
            m => m.Send(It.Is<ConfirmEmail.Command>(c => c.Token == "valid-token"), It.IsAny<CancellationToken>()),
            Times.Once,
            "Mediator should be called with the token");
    }

    #endregion

    #region Error State Tests

    [Fact]
    public async Task ConfirmEmail_WhenValidationException_ShouldShowErrorView()
    {
        // Arrange
        const string errorMessage = "Почта уже подтверждена";
        _ctx.MediatorMock
            .Setup(m => m.Send(It.IsAny<ConfirmEmail.Command>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException(errorMessage));

        NavigateToConfirmEmail("already-confirmed-token");

        // Act
        var cut = _ctx.RenderComponent<Schedia.Web.Shared.Pages.ConfirmEmail.ConfirmEmail>();

        // Wait for OnAfterRenderAsync to complete
        await Task.Delay(200);
        cut.Render();

        // Assert
        cut.ContainsText("Ошибка подтверждения").Should().BeTrue("Error title should be displayed");
        cut.ContainsText(errorMessage).Should().BeTrue("Error message should be displayed");
    }

    [Fact]
    public async Task ConfirmEmail_WhenError_ShouldShowCrossmark()
    {
        // Arrange
        _ctx.MediatorMock
            .Setup(m => m.Send(It.IsAny<ConfirmEmail.Command>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException("Error"));

        NavigateToConfirmEmail("invalid-token");

        // Act
        var cut = _ctx.RenderComponent<Schedia.Web.Shared.Pages.ConfirmEmail.ConfirmEmail>();

        // Wait for OnAfterRenderAsync to complete
        await Task.Delay(200);
        cut.Render();

        // Assert
        cut.Markup.Should().Contain("✗", "Crossmark should be visible on error");
    }

    [Fact]
    public async Task ConfirmEmail_WhenError_ShouldShowBackToLoginButton()
    {
        // Arrange
        _ctx.MediatorMock
            .Setup(m => m.Send(It.IsAny<ConfirmEmail.Command>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException("Error"));

        NavigateToConfirmEmail("invalid-token");

        // Act
        var cut = _ctx.RenderComponent<Schedia.Web.Shared.Pages.ConfirmEmail.ConfirmEmail>();

        // Wait for OnAfterRenderAsync to complete
        await Task.Delay(200);
        cut.Render();

        // Assert
        cut.ContainsText("Войти").Should().BeTrue("Back to Login button should be present");
    }

    [Fact]
    public async Task ConfirmEmail_WhenUnexpectedException_ShouldShowGenericError()
    {
        // Arrange
        _ctx.MediatorMock
            .Setup(m => m.Send(It.IsAny<ConfirmEmail.Command>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Something went wrong"));

        NavigateToConfirmEmail("valid-token");

        // Act
        var cut = _ctx.RenderComponent<Schedia.Web.Shared.Pages.ConfirmEmail.ConfirmEmail>();

        // Wait for OnAfterRenderAsync to complete
        await Task.Delay(200);
        cut.Render();

        // Assert
        cut.ContainsText("Ошибка подтверждения").Should().BeTrue("Error title should be displayed");
        cut.ContainsText("непредвиденная ошибка").Should().BeTrue("Generic error message should be displayed");
    }

    #endregion

    #region Token Validation Tests

    [Fact]
    public async Task ConfirmEmail_WithoutToken_ShouldNotCallMediator()
    {
        // Arrange & Act
        NavigateToConfirmEmail(null);
        var cut = _ctx.RenderComponent<Schedia.Web.Shared.Pages.ConfirmEmail.ConfirmEmail>();

        // Wait for OnAfterRenderAsync to complete
        await Task.Delay(200);

        // Assert
        _ctx.MediatorMock.Verify(
            m => m.Send(It.IsAny<ConfirmEmail.Command>(), It.IsAny<CancellationToken>()),
            Times.Never,
            "Mediator should not be called when token is missing");
    }

    [Fact]
    public async Task ConfirmEmail_WithoutToken_ShouldShowErrorState()
    {
        // Arrange & Act
        NavigateToConfirmEmail(null);
        var cut = _ctx.RenderComponent<Schedia.Web.Shared.Pages.ConfirmEmail.ConfirmEmail>();

        // Wait for OnAfterRenderAsync to complete
        await Task.Delay(200);
        cut.Render();

        // Assert - should transition to error state (not loading, not success)
        cut.ContainsText("Подтверждение Вашего электронного адреса").Should().BeFalse("Should not be in loading state");
    }

    [Fact]
    public async Task ConfirmEmail_WithEmptyToken_ShouldNotCallMediator()
    {
        // Arrange & Act
        NavigateToConfirmEmail("");
        var cut = _ctx.RenderComponent<Schedia.Web.Shared.Pages.ConfirmEmail.ConfirmEmail>();

        // Wait for OnAfterRenderAsync to complete
        await Task.Delay(200);

        // Assert
        _ctx.MediatorMock.Verify(
            m => m.Send(It.IsAny<ConfirmEmail.Command>(), It.IsAny<CancellationToken>()),
            Times.Never,
            "Mediator should not be called when token is empty");
    }

    #endregion

    #region Navigation Tests

    [Fact]
    public async Task ConfirmEmail_SuccessGoToHome_ShouldNavigate()
    {
        // Arrange
        _ctx.MediatorMock
            .Setup(m => m.Send(It.IsAny<ConfirmEmail.Command>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(Unit.Value));

        NavigateToConfirmEmail("valid-token");
        var cut = _ctx.RenderComponent<Schedia.Web.Shared.Pages.ConfirmEmail.ConfirmEmail>();

        // Wait for OnAfterRenderAsync to complete
        await Task.Delay(200);
        cut.Render();

        // Act
        var goHomeButton = cut.FindAll("button").First(b => b.TextContent.Contains("На главную"));
        goHomeButton.Click();

        // Assert
        _ctx.Navigation.Uri.Should().Be("http://localhost/");
    }

    [Fact]
    public async Task ConfirmEmail_ErrorBackToLogin_ShouldNavigate()
    {
        // Arrange
        _ctx.MediatorMock
            .Setup(m => m.Send(It.IsAny<ConfirmEmail.Command>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException("Error"));

        NavigateToConfirmEmail("invalid-token");
        var cut = _ctx.RenderComponent<Schedia.Web.Shared.Pages.ConfirmEmail.ConfirmEmail>();

        // Wait for OnAfterRenderAsync to complete
        await Task.Delay(200);
        cut.Render();

        // Act
        var backButton = cut.FindAll("button").First(b => b.TextContent.Contains("Войти"));
        backButton.Click();

        // Assert
        _ctx.Navigation.Uri.Should().Contain("/login");
    }

    #endregion
}
