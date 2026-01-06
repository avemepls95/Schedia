namespace Avemepls.Domain.Exceptions;

/// <summary>
/// Доступ к сущности запрещен.
/// </summary>
public class AccessDeniedException : Exception
{
    public AccessDeniedException() : this("Access denied")
    {
    }

    public AccessDeniedException(string? message) : base(message)
    {
    }

    public AccessDeniedException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}