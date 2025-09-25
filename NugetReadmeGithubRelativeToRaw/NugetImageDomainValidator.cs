using System;
using System.Text.RegularExpressions;

namespace NugetReadmeGithubRelativeToRaw
{
    // https://github.com/NuGet/NuGetGallery/blob/main/src/NuGetGallery/Services/ImageDomainValidator.cs
    internal class NugetImageDomainValidator : INugetImageDomainValidator
    {
        private static readonly TimeSpan RegexTimeout = TimeSpan.FromMinutes(1);
        private static readonly Regex GithubBadgeUrlRegEx = new Regex("^(https|http):\\/\\/github\\.com\\/[^/]+\\/[^/]+(\\/actions)?\\/workflows\\/.*badge\\.svg", RegexOptions.IgnoreCase, RegexTimeout);
        private readonly INugetTrustedImageDomains _trustedImageDomains;

        public NugetImageDomainValidator(INugetTrustedImageDomains trustedImageDomains)
            => _trustedImageDomains = trustedImageDomains;

        public bool IsTrustedImageDomain(string uriString)
        {
            if (Uri.TryCreate(uriString, UriKind.Absolute, out var uri))
            {
                if (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
                {
                    return IsTrustedImageDomain(uri);
                }
            }
            return false;
        }

        private bool IsTrustedImageDomain(Uri uri)
        {
            return _trustedImageDomains.IsImageDomainTrusted(uri.Host) ||
                IsGitHubBadge(uri);
        }

        private bool IsGitHubBadge(Uri uri)
        {
            try
            {
                return GithubBadgeUrlRegEx.IsMatch(uri.OriginalString);
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }
    }
}
