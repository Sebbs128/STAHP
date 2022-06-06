using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Whois;

namespace Stahp.Core.HostTypes
{
    internal class HostFactory : IHostFactory
    {
        private readonly WhoisLookup _whoisClient;
        private readonly ILogger<HostFactory> _logger;

        public HostFactory(WhoisLookup whoisClient, ILogger<HostFactory> logger)
        {
            _whoisClient = whoisClient;
            _logger = logger;
        }

        public async Task<IHost> GetHost(Uri uri)
        {
            // TODO: this is lacking. use a command pattern in a proper factory?
            // eg. Regex match on known formats for AWS and Azure?
            // this is also specifically doing a whois lookup if it's not a known host type
            // there will later need to be some nslookup-equivalent lookup in addition,
            //  requiring splitting this method into Registrar lookup and hosting lookup
            if (uri.Host.EndsWith(".amazonaws.com", StringComparison.OrdinalIgnoreCase))
            {
                return new AmazonS3BucketHost(uri);
            }
            if (uri.Host.EndsWith(".blob.core.windows.net", StringComparison.OrdinalIgnoreCase))
            {
                return new AzureStorageBlobHost(uri);
            }

            // do a whois lookup to determine host and abuse contact details
            WhoisResponse? whoisResponse = await DoWhoisLookup(uri.Host);

            return new NonspecificHost
            {
                HostName = whoisResponse?.Registrar?.Name ?? "",
                HostUrl = whoisResponse?.Registrar?.Url ?? "",
                AbuseContact = whoisResponse?.Registrar?.AbuseEmail ?? "",
            };
        }

        private async Task<WhoisResponse?> DoWhoisLookup(string domainName)
        {
            try
            {
                // TODO: consider possibly using Polly to retry with longer timeouts (to a limit)
                WhoisRequest request = new(domainName)
                {
                    TimeoutSeconds = 120
                };
                WhoisResponse? response = await _whoisClient.LookupAsync(request);

                if (response?.Registrar is null)
                {
                    response = await DoWhoisLookup(domainName[(domainName.IndexOf('.') + 1)..]);
                }
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Encountered an error performing whois query on {domain}", domainName);
            }
            return null;
        }
    }
}
