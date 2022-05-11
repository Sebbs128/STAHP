namespace Stahp.Core.HostTypes
{
    public record NonspecificHost : IHost
    {
        public string HostName { get; init; }
        public string AbuseContact { get; init; }
        public string HostUrl { get; init; }
    }
}
