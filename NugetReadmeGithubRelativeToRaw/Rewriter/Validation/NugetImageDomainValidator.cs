using System;

namespace NugetReadmeGithubRelativeToRaw.Rewriter.Validation
{
    // https://github.com/NuGet/NuGetGallery/blob/main/src/NuGetGallery/Services/ImageDomainValidator.cs
    internal class NuGetImageDomainValidator : INuGetImageDomainValidator
    {
        
        private readonly INuGetTrustedImageDomains _trustedImageDomains;
        private readonly INuGetGitHubBadgeValidator _nugetGitHubBadgeValidator;

        public NuGetImageDomainValidator(INuGetTrustedImageDomains trustedImageDomains, INuGetGitHubBadgeValidator nugetGitHubBadgeValidator)
        {
            _trustedImageDomains = trustedImageDomains;
            _nugetGitHubBadgeValidator = nugetGitHubBadgeValidator;
        }

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
                _nugetGitHubBadgeValidator.Validate(uri.OriginalString);
        }
    }
}
