namespace Schedia.Web.Shared.Services;

public interface IPlatformService
{
    bool IsMaui { get; }
    bool IsWeb { get; }
    bool IsAndroid { get; }
    bool IsIOS { get; }
    string Platform { get; }
}