namespace Avemepls.ServiceBus.Common;

public class CrossDomainRequestError
{
    public CrossDomainRequestError(Exception ex)
    {
        Message = ex.Message;
        Details = ex.ToString();
        SourceExceptionTypeName = ex.GetType().FullName;
    }

    public CrossDomainRequestError(string message)
    {
        Message = message;
    }

    public CrossDomainRequestError()
    {
    }

    public string Message { get; set; }

    public string? Details { get; set; }

    public string? SourceExceptionTypeName { get; set; }
}