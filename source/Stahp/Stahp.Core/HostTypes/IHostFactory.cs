namespace Stahp.Core.HostTypes
{
    public interface IHostFactory
    {
        Task<IHost> GetHost(Uri uri);
    }
}