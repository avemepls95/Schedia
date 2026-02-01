namespace Schedia.Web.Base.Configs;

public class DistributedCacheConfig
{
    public const string ConfigKey = "Caching";

    public string Host { get; set; }

    public int Port { get; set; }

    public int DefaultDataBase { get; set; }
}