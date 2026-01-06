using System.Runtime.ExceptionServices;

using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Avemepls.Mvc.ModelBinding;

/// <summary>
/// Model binder to convert unix timestamps into <see cref="DateTime"/> and <see cref="DateTimeOffset"/>.
/// </summary>
public class UnixDateTimeModelBinder : IModelBinder
{
    /// <inheritdoc />
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext);

        var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
        if (valueProviderResult == ValueProviderResult.None)
        {
            return Task.CompletedTask;
        }

        bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);

        try
        {
            var value = valueProviderResult.FirstValue;

            object? model;

            if (bindingContext.ModelType == typeof(DateTime))
            {
                model = DateTime.UnixEpoch.AddSeconds(Convert.ToInt32(value));
            }
            else if (bindingContext.ModelType == typeof(DateTimeOffset))
            {
                model = DateTimeOffset.UnixEpoch.AddSeconds(Convert.ToInt32(value));
            }
            else
            {
                model = null;
            }

            CheckModel(bindingContext, valueProviderResult, model);

            return Task.CompletedTask;
        }
        catch (Exception exception)
        {
            var isFormatException = exception is FormatException;
            if (!isFormatException && exception.InnerException != null)
            {
                exception = ExceptionDispatchInfo.Capture(exception.InnerException).SourceException;
            }

            bindingContext.ModelState.TryAddModelError(
                bindingContext.ModelName,
                exception,
                bindingContext.ModelMetadata);

            return Task.CompletedTask;
        }
    }

    protected virtual void CheckModel(
        ModelBindingContext bindingContext,
        ValueProviderResult valueProviderResult,
        object? model)
    {
        if (model is null && !bindingContext.ModelMetadata.IsReferenceOrNullableType)
        {
            bindingContext.ModelState.TryAddModelError(
                bindingContext.ModelName,
                bindingContext.ModelMetadata.ModelBindingMessageProvider.ValueMustNotBeNullAccessor(
                    valueProviderResult.ToString()));
        }
        else
        {
            bindingContext.Result = ModelBindingResult.Success(model);
        }
    }
}