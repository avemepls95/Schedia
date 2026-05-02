using Schedia.Web.Blazor.Endpoints;

namespace Schedia.Web.Blazor.Tests.Endpoints;

public class ReturnUrlValidatorTests
{
    [Theory]
    [InlineData("/")]
    [InlineData("/foo")]
    [InlineData("/foo/bar")]
    [InlineData("/foo?x=1")]
    [InlineData("~/")]
    [InlineData("~/foo")]
    public void IsLocalUrl_Should_ReturnTrue_When_UrlIsLocal(string url)
    {
        // Arrange & Act
        var result = ReturnUrlValidator.IsLocalUrl(url);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("https://evil.com")]
    [InlineData("http://evil.com")]
    [InlineData("//evil.com")]
    [InlineData("/\\evil.com")]
    [InlineData("~//evil.com")]
    [InlineData("~/\\evil.com")]
    [InlineData("javascript:alert(1)")]
    [InlineData("foo")]
    public void IsLocalUrl_Should_ReturnFalse_When_UrlIsNotLocal(string? url)
    {
        // Arrange & Act
        var result = ReturnUrlValidator.IsLocalUrl(url);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GetSafeReturnUrl_Should_ReturnUrl_When_UrlIsLocal()
    {
        // Arrange
        const string url = "/dashboard";

        // Act
        var result = ReturnUrlValidator.GetSafeReturnUrl(url);

        // Assert
        result.Should().Be(url);
    }

    [Fact]
    public void GetSafeReturnUrl_Should_ReturnFallback_When_UrlIsExternal()
    {
        // Arrange
        const string url = "https://evil.com";

        // Act
        var result = ReturnUrlValidator.GetSafeReturnUrl(url);

        // Assert
        result.Should().Be("/");
    }

    [Fact]
    public void GetSafeReturnUrl_Should_ReturnFallback_When_UrlIsNull()
    {
        // Arrange & Act
        var result = ReturnUrlValidator.GetSafeReturnUrl(null);

        // Assert
        result.Should().Be("/");
    }

    [Fact]
    public void GetSafeReturnUrl_Should_ReturnCustomFallback_When_UrlIsInvalid()
    {
        // Arrange & Act
        var result = ReturnUrlValidator.GetSafeReturnUrl("//evil.com", "/home");

        // Assert
        result.Should().Be("/home");
    }
}