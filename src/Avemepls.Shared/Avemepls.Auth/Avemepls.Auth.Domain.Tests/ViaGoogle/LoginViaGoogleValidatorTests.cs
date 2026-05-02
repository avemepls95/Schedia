using Avemepls.Auth.Domain.ViaGoogle;
using FluentValidation.TestHelper;

namespace Avemepls.Auth.Domain.Tests.ViaGoogle;

public class LoginViaGoogleValidatorTests
{
    private readonly LoginViaGoogle.Validator _validator = new();

    [Fact]
    public void Validate_Should_Pass_When_CommandIsValid()
    {
        // Arrange
        var command = new LoginViaGoogle.Command
        {
            GoogleId = "google-123",
            Email = "user@example.com",
            FirstName = "First",
            LastName = "Last",
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_Should_Fail_When_GoogleIdIsEmpty(string googleId)
    {
        // Arrange
        var command = new LoginViaGoogle.Command
        {
            GoogleId = googleId,
            Email = "user@example.com",
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.GoogleId);
    }

    [Fact]
    public void Validate_Should_Fail_When_EmailIsEmpty()
    {
        // Arrange
        var command = new LoginViaGoogle.Command
        {
            GoogleId = "google-123",
            Email = string.Empty,
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("user@")]
    [InlineData("@example.com")]
    public void Validate_Should_Fail_When_EmailIsInvalid(string email)
    {
        // Arrange
        var command = new LoginViaGoogle.Command
        {
            GoogleId = "google-123",
            Email = email,
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }
}
