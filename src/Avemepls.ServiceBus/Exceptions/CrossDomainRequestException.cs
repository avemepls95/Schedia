namespace Avemepls.ServiceBus.Exceptions;

public class CrossDomainRequestException(string message, string? details = null, string? sourceExceptionTypeName = null) : Exception(message)
{
    public string? SourceExceptionTypeName { get; set; } = sourceExceptionTypeName;

    public string? Details { get; } = details;
}