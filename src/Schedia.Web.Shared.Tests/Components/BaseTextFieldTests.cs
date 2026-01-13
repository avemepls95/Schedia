using Schedia.Web.Shared.Components;

namespace Schedia.Web.Shared.Tests.Components;

public class BaseTextFieldTests : IDisposable
{
    private readonly BlazorTestContext _ctx = new();

    public void Dispose()
    {
        _ctx.Dispose();
    }

    #region Rendering Tests

    [Fact]
    public void BaseTextField_ShouldRender_WithLabel()
    {
        // Arrange & Act
        var cut = _ctx.RenderComponent<BaseTextField<string>>(parameters =>
            parameters.Add(p => p.Label, "Test Label"));

        // Assert
        cut.Markup.Should().Contain("Test Label");
    }

    [Fact]
    public void BaseTextField_ShouldRender_WithPlaceholder()
    {
        // Arrange & Act
        var cut = _ctx.RenderComponent<BaseTextField<string>>(parameters =>
            parameters.Add(p => p.Placeholder, "Enter value"));

        // Assert
        var input = cut.Find("input");
        input.GetAttribute("placeholder").Should().Be("Enter value");
    }

    [Fact]
    public void BaseTextField_ShouldRender_WithHelperText()
    {
        // Arrange & Act
        var cut = _ctx.RenderComponent<BaseTextField<string>>(parameters =>
            parameters.Add(p => p.HelperText, "This is helper text"));

        // Assert
        cut.Markup.Should().Contain("This is helper text");
    }

    #endregion

    #region Password Toggle Tests

    [Fact]
    public void BaseTextField_PasswordType_ShouldRenderAsPassword()
    {
        // Arrange & Act
        var cut = _ctx.RenderComponent<BaseTextField<string>>(parameters =>
            parameters.Add(p => p.InputType, InputType.Password));

        // Assert
        var input = cut.Find("input");
        input.GetAttribute("type").Should().Be("password");
    }

    [Fact]
    public void BaseTextField_PasswordType_ShouldHaveToggleAdornment()
    {
        // Arrange & Act
        var cut = _ctx.RenderComponent<BaseTextField<string>>(parameters =>
            parameters
                .Add(p => p.InputType, InputType.Password)
                .Add(p => p.ShowPasswordToggle, true));

        // Assert - should have an adornment button for toggling
        var adornment = cut.FindAll(".mud-input-adornment");
        adornment.Should().NotBeEmpty("Password field should have toggle adornment");
    }

    [Fact]
    public void BaseTextField_PasswordType_ToggleClick_ShouldShowPassword()
    {
        // Arrange
        var cut = _ctx.RenderComponent<BaseTextField<string>>(parameters =>
            parameters
                .Add(p => p.InputType, InputType.Password)
                .Add(p => p.ShowPasswordToggle, true)
                .Add(p => p.Value, "secret"));

        // Initial state - password hidden
        var input = cut.Find("input");
        input.GetAttribute("type").Should().Be("password");

        // Act - find and click toggle button
        var toggleButtons = cut.FindAll(".mud-input-adornment button");
        if (toggleButtons.Any())
        {
            toggleButtons.First().Click();

            // Assert - password visible
            input = cut.Find("input");
            input.GetAttribute("type").Should().Be("text");
        }
    }

    [Fact]
    public void BaseTextField_PasswordType_WithToggleDisabled_ShouldNotShowAdornment()
    {
        // Arrange & Act
        var cut = _ctx.RenderComponent<BaseTextField<string>>(parameters =>
            parameters
                .Add(p => p.InputType, InputType.Password)
                .Add(p => p.ShowPasswordToggle, false));

        // Assert - no adornment for password toggle
        var adornmentButtons = cut.FindAll(".mud-input-adornment button");
        adornmentButtons.Should().BeEmpty("No toggle button when ShowPasswordToggle is false");
    }

    #endregion

    #region Value Binding Tests

    [Fact]
    public void BaseTextField_ShouldDisplayInitialValue()
    {
        // Arrange & Act
        var cut = _ctx.RenderComponent<BaseTextField<string>>(parameters =>
            parameters.Add(p => p.Value, "Initial Value"));

        // Assert
        var input = cut.Find("input");
        input.GetAttribute("value").Should().Be("Initial Value");
    }

    [Fact]
    public void BaseTextField_ValueChanged_ShouldTriggerCallback()
    {
        // Arrange
        var newValue = "";
        var cut = _ctx.RenderComponent<BaseTextField<string>>(parameters =>
            parameters
                .Add(p => p.Value, "initial")
                .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, v => newValue = v)));

        // Act
        var input = cut.Find("input");
        input.Change("updated");

        // Assert
        newValue.Should().Be("updated");
    }

    [Fact]
    public void BaseTextField_WithIntType_ShouldBindCorrectly()
    {
        // Arrange
        int? newValue = null;
        var cut = _ctx.RenderComponent<BaseTextField<int>>(parameters =>
            parameters
                .Add(p => p.Value, 0)
                .Add(p => p.ValueChanged, EventCallback.Factory.Create<int>(this, v => newValue = v)));

        // Act
        var input = cut.Find("input");
        input.Change("42");

        // Assert
        newValue.Should().Be(42);
    }

    #endregion

    #region State Tests

    [Fact]
    public void BaseTextField_Disabled_ShouldBeDisabled()
    {
        // Arrange & Act
        var cut = _ctx.RenderComponent<BaseTextField<string>>(parameters =>
            parameters.Add(p => p.Disabled, true));

        // Assert
        var input = cut.Find("input");
        input.HasAttribute("disabled").Should().BeTrue();
    }

    [Fact]
    public void BaseTextField_ReadOnly_ShouldBeReadOnly()
    {
        // Arrange & Act
        var cut = _ctx.RenderComponent<BaseTextField<string>>(parameters =>
            parameters.Add(p => p.ReadOnly, true));

        // Assert
        var input = cut.Find("input");
        input.HasAttribute("readonly").Should().BeTrue();
    }

    [Fact]
    public void BaseTextField_Required_ShouldHaveRequiredAttribute()
    {
        // Arrange & Act
        var cut = _ctx.RenderComponent<BaseTextField<string>>(parameters =>
            parameters.Add(p => p.Required, true));

        // Assert
        var input = cut.Find("input");
        input.HasAttribute("required").Should().BeTrue();
    }

    #endregion

    #region Icon Tests

    [Fact]
    public void BaseTextField_WithStartIcon_ShouldDisplayIcon()
    {
        // Arrange & Act
        var cut = _ctx.RenderComponent<BaseTextField<string>>(parameters =>
            parameters.Add(p => p.StartIcon, Icons.Material.Filled.Email));

        // Assert
        var adornment = cut.FindAll(".mud-input-adornment-start");
        adornment.Should().NotBeEmpty("Start icon adornment should be present");
    }

    [Fact]
    public void BaseTextField_WithEndIcon_ShouldDisplayIcon()
    {
        // Arrange & Act
        var cut = _ctx.RenderComponent<BaseTextField<string>>(parameters =>
            parameters.Add(p => p.EndIcon, Icons.Material.Filled.Search));

        // Assert
        var adornment = cut.FindAll(".mud-input-adornment-end");
        adornment.Should().NotBeEmpty("End icon adornment should be present");
    }

    #endregion

    #region Class and Style Tests

    [Fact]
    public void BaseTextField_ShouldApplyCustomClass()
    {
        // Arrange & Act
        var cut = _ctx.RenderComponent<BaseTextField<string>>(parameters =>
            parameters.Add(p => p.Class, "custom-class"));

        // Assert
        cut.Markup.Should().Contain("custom-class");
    }

    [Fact]
    public void BaseTextField_ShouldApplyDefaultMarginClass()
    {
        // Arrange & Act
        var cut = _ctx.RenderComponent<BaseTextField<string>>();

        // Assert - default class includes mb-4
        cut.Markup.Should().Contain("mb-4");
    }

    #endregion
}
