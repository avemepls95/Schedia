using Schedia.Web.Base.Services;

namespace Schedia.Web.Blazor.Services;

public class WebPlatformService : IPlatformService
{
    public bool IsMaui => false;
    public bool IsWeb => true;
    public bool IsAndroid => false;
    public bool IsIOS => false;
    public string Platform => "Web";
}