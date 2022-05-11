namespace Stahp.Core.HostTypes
{
    public interface IHost
    {
        string HostName { get; }
        string HostUrl { get; }
        string AbuseContact { get; }
    }
}
