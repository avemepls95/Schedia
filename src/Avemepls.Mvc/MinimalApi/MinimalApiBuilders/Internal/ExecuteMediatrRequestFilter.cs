namespace Avemepls.Mvc.MinimalApi.MinimalApiBuilders.Internal;

/// <summary>
/// Отправляет запрос в медиатор и возвращает ответ, нужен для маппинга HTTP запроса в запрос Mediatr
/// </summary>
public class ExecuteMediatrRequestFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var request = await next.Invoke(context);
        var mediatr = context.HttpContext.RequestServices.GetRequiredService<IMediator>();

        var response = await mediatr.Send(request!);

        return response;
    }
}