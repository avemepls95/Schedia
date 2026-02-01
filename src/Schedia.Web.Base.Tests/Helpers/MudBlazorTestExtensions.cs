using AngleSharp.Dom;

namespace Schedia.Web.Base.Tests.Helpers;

/// <summary>
/// Extension methods for testing MudBlazor components with bUnit.
/// </summary>
public static class MudBlazorTestExtensions
{
    /// <summary>
    /// Finds a MudTextField input by its label text.
    /// </summary>
    public static IElement FindInputByLabel(this IRenderedFragment component, string label)
    {
        // MudTextField renders label inside .mud-input-label
        var labels = component.FindAll(".mud-input-label");
        foreach (var labelElement in labels)
        {
            if (labelElement.TextContent.Contains(label))
            {
                // Find the parent mud-input-control and then the input
                var parent = labelElement.ParentElement;
                while (parent != null && !parent.ClassList.Contains("mud-input-control"))
                {
                    parent = parent.ParentElement;
                }

                var input = parent?.QuerySelector("input");
                if (input != null)
                {
                    return input;
                }
            }
        }

        // Fallback: try to find by placeholder or aria-label
        var inputs = component.FindAll("input");
        foreach (var input in inputs)
        {
            if (input.GetAttribute("placeholder")?.Contains(label) == true ||
                input.GetAttribute("aria-label")?.Contains(label) == true)
            {
                return input;
            }
        }

        throw new ElementNotFoundException($"Input with label '{label}' not found");
    }

    /// <summary>
    /// Finds a MudButton by its text content.
    /// </summary>
    public static IElement FindButtonByText(this IRenderedFragment component, string text)
    {
        var buttons = component.FindAll("button");
        foreach (var button in buttons)
        {
            if (button.TextContent.Contains(text, StringComparison.OrdinalIgnoreCase))
            {
                return button;
            }
        }

        throw new ElementNotFoundException($"Button with text '{text}' not found");
    }

    /// <summary>
    /// Finds a MudAlert component by severity.
    /// </summary>
    public static IElement? FindAlert(this IRenderedFragment component, Severity severity)
    {
        var severityClass = severity switch
        {
            Severity.Error => "mud-alert-text-error",
            Severity.Success => "mud-alert-text-success",
            Severity.Warning => "mud-alert-text-warning",
            Severity.Info => "mud-alert-text-info",
            _ => ""
        };

        var alerts = component.FindAll(".mud-alert");
        return alerts.FirstOrDefault(a =>
            a.ClassList.Contains(severityClass) ||
            a.ClassList.Contains($"mud-alert-filled-{severity.ToString().ToLower()}"));
    }

    /// <summary>
    /// Checks if a loading indicator is visible.
    /// Checks for MudProgressCircular, MudProgressLinear, loading buttons, or loading text.
    /// </summary>
    public static bool HasLoadingIndicator(this IRenderedFragment component)
    {
        // Check for various loading patterns
        return component.FindAll(".mud-progress-circular").Any() ||
               component.FindAll(".mud-progress-linear").Any() ||
               component.FindAll(".mud-button-loading").Any() ||
               component.FindAll("[disabled]").Any(e => e.TextContent.Contains("Loading", StringComparison.OrdinalIgnoreCase)) ||
               component.Markup.Contains("Loading", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Sets the value of a MudTextField and triggers blur event.
    /// </summary>
    public static void SetInputValue(this IRenderedFragment component, string label, string value)
    {
        var input = component.FindInputByLabel(label);
        input.Change(value);
        input.Blur();
    }

    /// <summary>
    /// Sets the value of an input by index.
    /// </summary>
    public static void SetInputValueByIndex(this IRenderedFragment component, int index, string value)
    {
        var inputs = component.FindAll("input");
        if (index >= inputs.Count)
        {
            throw new IndexOutOfRangeException($"Input index {index} is out of range. Found {inputs.Count} inputs.");
        }

        inputs[index].Change(value);
        inputs[index].Blur();
    }

    /// <summary>
    /// Clicks a button by its text content.
    /// </summary>
    public static void ClickButton(this IRenderedFragment component, string text)
    {
        var button = component.FindButtonByText(text);
        button.Click();
    }

    /// <summary>
    /// Finds a link by its href attribute.
    /// </summary>
    public static IElement FindLinkByHref(this IRenderedFragment component, string href)
    {
        var links = component.FindAll("a");
        return links.FirstOrDefault(l => l.GetAttribute("href")?.Contains(href) == true)
            ?? throw new ElementNotFoundException($"Link with href '{href}' not found");
    }

    /// <summary>
    /// Checks if component contains specific text.
    /// </summary>
    public static bool ContainsText(this IRenderedFragment component, string text)
    {
        return component.Markup.Contains(text, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Asserts that a validation error message is displayed.
    /// </summary>
    public static void AssertHasValidationError(this IRenderedFragment component, string expectedMessage)
    {
        var errorElements = component.FindAll(".mud-input-helper-text, .validation-message, .mud-input-error");
        var hasError = errorElements.Any(e => e.TextContent.Contains(expectedMessage, StringComparison.OrdinalIgnoreCase));
        hasError.Should().BeTrue($"Expected validation error '{expectedMessage}' was not found");
    }

    /// <summary>
    /// Gets the count of input fields in the component.
    /// </summary>
    public static int GetInputCount(this IRenderedFragment component)
    {
        return component.FindAll("input").Count;
    }
}

/// <summary>
/// Exception thrown when an element is not found in the rendered component.
/// </summary>
public class ElementNotFoundException(string message) : Exception(message);
