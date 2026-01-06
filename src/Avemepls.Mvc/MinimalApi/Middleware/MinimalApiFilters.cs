namespace Avemepls.Mvc.MinimalApi.Middleware;

public static class MinimalApiFilters
{
    public static readonly AccessDeniedExceptionEndpointFilter AccessDeniedExceptionEndpointFilter = new();
    public static readonly ApplicationExceptionEndpointFilter ApplicationExceptionEndpointFilter = new();
    public static readonly ConcurrencyEndpointFilter<int> ConcurrencyEndpointFilterInt = new();
    public static readonly ObjectNotFoundEndpointFilter<int> ObjectNotFoundEndpointFilterInt = new();
    public static readonly ListValidationExceptionEndpointFilter ListValidationExceptionEndpointFilter = new();
    public static readonly ObjectNotFoundEndpointFilter ObjectNotFoundEndpointFilter = new();
    public static readonly OperationCancelledEndpointFilter OperationCancelledEndpointFilter = new();
    public static readonly UserConfirmationContextEndpointFilter UserConfirmationContextEndpointFilter = new();
    public static readonly ValidationEndpointFilter ValidationEndpointFilter = new();
}