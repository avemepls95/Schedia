namespace Avemepls.Core.DataAccess.ContextInitializing;

public interface IContextInitializer : IDisposable
{
    public void Initialize();
}