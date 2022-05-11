using System.Diagnostics.CodeAnalysis;

namespace Stahp.Core.HostTypes
{
    // https://bucket-name.s3.Region.amazonaws.com/key-name
    // https://s3.Region.amazonaws.com/bucket-name/key-name
    public record AmazonS3BucketHost : IHost
    {
        public string HostName => "Amazon Web Services";
        public string BucketName { get; private set; }
        public string Region { get; private set; }
        public string AbuseContact => "abuse@amazonaws.com";
        public string HostUrl => "https://support.aws.amazon.com/#/contacts/report-abuse/";

        public AmazonS3BucketHost(string url)
        {
            ParseDataFromUrl(url);
        }

        public AmazonS3BucketHost(Uri uri)
        {
            ParseDataFromUrl(uri.ToString());
        }

        [MemberNotNull(nameof(BucketName))]
        [MemberNotNull(nameof(Region))]
        private void ParseDataFromUrl(string url)
        {
            string[] stringParts = url
                .Replace("http://", "", StringComparison.OrdinalIgnoreCase)
                .Replace("https://", "", StringComparison.OrdinalIgnoreCase)
                .Split('.', '/');

            int regionIndex = 1;
            if (!string.Equals("s3", stringParts[0], StringComparison.OrdinalIgnoreCase))
            {
                regionIndex++;
                BucketName = stringParts[0];
            }
            else
            {
                BucketName = stringParts[4];
            }
            Region = stringParts[regionIndex];
        }
    }
}
