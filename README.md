# STAHP

Site Tracing And Host Program follows and reports on the redirection chain for a phishing site.

## Usage

To start a trace on a single URL

`stahp.console.exe -u|--url <url>`

eg.

`stahp.console.exe --url example.com`

STAHP can also be run in an interactive mode by not providing and command line arguments, allowing you to perform subsequent traces.

## What?

Today's phishing and other broadly-cast scam attacks often involve directing a user through multiple URLs before reaching the target site.

STAHP is a .NET 6 console application to follow and record the various redirect methods used between the initial hyperlink sent to a target, and the eventual target site used for the phishing or other scam attack. Along the way, it will pull out relevant information for the cloud provider, or perform `whois` queries to find where a domain name is registered, and where to report abuse to those companies.

You could of course do the same thing using browser dev tools to see each redirect and do `whois` queries yourself, but this takes that legwork out for you, also saving you time.

## Why?

 Scammers are thrifty and agile, and don't expect all their infrastructure to remain operational for long.

 The free storage tiers offered by cloud providers can easily host static websites to redirect users through to the eventual target site, providing further obfuscation to casual observers. Further, these services can be updated very quickly and easily if a link in the chain breaks (or to purposefully break the chain to hide the target site).

The quicker the use of these services can be reported for malicious activity to any organizations involved, and taken down, the fewer victims there are that may fall prey to the attack.

## Future work/ideas

- More cloud providers and services
  - Currently only specifically identifies AWS S3 and Azure Blobs
- Support for `nslookup` with non-authoritative answers to identify and report on web hosting providers
- Microsoft Security Response Center offers an API at https://msrc.microsoft.com/report/developer for reporting abuse of Microsoft online services. It would be good to integrate with this to allow single-step reporting within STAHP of any and all Azure services found within a trace.
- Identify and follow JavaScript-based redirections (`window.location`)
- Web interface
- Display hops in the trace in real-time
- Improve known-host matching
  - Related but partly opposite to this is investigating determining if a legit URL is found in the trace, such as URL obfuscators in email hosts/applications
- Automated builds of console application (Win x86/x64/ARM(?), MacOS, Linux)
  - Single-file and self-contained executable would be ideal. See https://docs.microsoft.com/en-us/dotnet/core/deploying/single-file/overview#publish-a-single-file-app---sample-project-file
- Publish `Stahp.Core` as a nuget package to allow other projects to make use of it.
  - Further to this, dependencies for whois, html parsing, nslookup etc could/may be updated, abstracted, or replaced for better framework (ie. .NET target) support