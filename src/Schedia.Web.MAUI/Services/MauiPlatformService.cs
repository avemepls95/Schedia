using Schedia.Web.Shared.Services;

namespace Schedia.Web.MAUI.Services;

public class MauiPlatformService : IPlatformService
{
    public bool IsMaui => true;
    public bool IsWeb => false;

    public bool IsAndroid =>
#if ANDROID
        true;
#else
        false;
#endif

    public bool IsIOS =>
#if IOS
        true;
#else
        false;
#endif

    public string Platform
    {
        get
        {
#if ANDROID
            return "Android";
#elif IOS
            return "iOS";
#else
            return "Unknown";
#endif
        }
    }
}
