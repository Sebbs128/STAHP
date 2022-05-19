namespace Stahp.Core.HostTypes
{
    public record NonspecificHost : IHost
    {
        public string HostName { get; init; } = string.Empty;
        public string AbuseContact { get; init; } = string.Empty;
        public string HostUrl { get; init; } = string.Empty;
    }
}
