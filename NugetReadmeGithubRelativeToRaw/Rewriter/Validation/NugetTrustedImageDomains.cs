using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace NugetReadmeGithubRelativeToRaw.Rewriter.Validation
{
    [ExcludeFromCodeCoverage]
    internal class NuGetTrustedImageDomains : INuGetTrustedImageDomains
    {
        // https://github.com/NuGet/NuGetGallery/blob/2e04a148fa40d004f40cd648c83b35c4b264cfd9/src/NuGetGallery/App_Data/Files/Content/Trusted-Image-Domains.json
        private static readonly string[] s_trustedImageDomainList = new string[]
        {
            "api.codacy.com",
            "app.codacy.com",
            "api.codeclimate.com",
            "app.deepsource.com",
            "api.dependabot.com",
            "api.travis-ci.com",
            "api.reuse.software",
            "badgen.net",
            "badges.gitter.im",
            "caniuse.bitsofco.de",
            "cdn.jsdelivr.net",
            "cdn.syncfusion.com",
            "ci.appveyor.com",
            "circleci.com",
            "cloudback.it",
            "codecov.io",
            "codefactor.io",
            "coveralls.io",
            "dev.azure.com",
            "flat.badgen.net",
            "gitlab.com",
            "img.shields.io",
            "infragistics.com",
            "i.imgur.com",
            "isitmaintained.com",
            "media.githubusercontent.com",
            "opencollective.com",
            "snyk.io",
            "sonarcloud.io",
            "travis-ci.com",
            "travis-ci.org",
            "avatars.githubusercontent.com",
            "raw.github.com",
            "raw.githubusercontent.com",
            "user-images.githubusercontent.com",
            "camo.githubusercontent.com"
        };
        private static readonly HashSet<string> s_trustedImageDomains = new HashSet<string>(s_trustedImageDomainList, StringComparer.OrdinalIgnoreCase);

        private NuGetTrustedImageDomains() { }

        public static NuGetTrustedImageDomains Instance { get; } = new NuGetTrustedImageDomains();
        
        public bool IsImageDomainTrusted(string imageDomain)
        {
            if (imageDomain == null)
            {
                return false;
            }

            return s_trustedImageDomains.Contains(imageDomain);

        }
    }
}
