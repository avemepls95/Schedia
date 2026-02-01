namespace Avemepls.ServiceBus.Exceptions;

public static class CrossDomainRequestExceptionExtensions
{
    public static bool Is<TException>(this CrossDomainRequestException exception)
        where TException : Exception
    {
        return exception.SourceExceptionTypeName == typeof(TException).FullName;
    }
}