using System.Diagnostics.CodeAnalysis;

namespace Stahp.Core.HostTypes
{
    // https://accountName.blob.core.windows.net/containername

    // TODO: integrate with https://msrc.microsoft.com/report/developer?
    public record AzureStorageBlobHost : IHost
    {
        public string AccountName { get; private set; }
        public string ContainerName { get; private set; }
        public string HostName => "Microsoft Azure";
        public string AbuseContact => string.Empty;
        public string HostUrl => "https://msrc.microsoft.com/report/abuse";

        public AzureStorageBlobHost(string url)
        {
            ParseDataFromUrl(url);
        }

        public AzureStorageBlobHost(Uri uri)
        {
            ParseDataFromUrl(uri.ToString());
        }

        [MemberNotNull(nameof(AccountName))]
        [MemberNotNull(nameof(ContainerName))]
        private void ParseDataFromUrl(string url)
        {
            string[] stringParts = url
                .Replace("http://", "", StringComparison.OrdinalIgnoreCase)
                .Replace("https://", "", StringComparison.OrdinalIgnoreCase)
                .Split('.', '/');

            AccountName = stringParts[0];
            ContainerName = stringParts[5];
        }
    }
}
