namespace Avemepls.ServiceBus.Helpers;

public static class EndpointHelper
{
    public static string GenerateEndpointName(Type type)
    {
        return "request:" + type.FullName!.Replace('.', '_').ToLower();
    }
}