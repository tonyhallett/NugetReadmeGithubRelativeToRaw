using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace NugetReadmeGithubRelativeToRaw.Rewriter.Validation
{
    [ExcludeFromCodeCoverage]
    internal class NuGetGitHubBadgeValidator : INuGetGitHubBadgeValidator
    {
        private static readonly TimeSpan RegexTimeout = TimeSpan.FromMinutes(1);
        private static readonly Regex GitHubBadgeUrlRegEx = new Regex("^(https|http):\\/\\/github\\.com\\/[^/]+\\/[^/]+(\\/actions)?\\/workflows\\/.*badge\\.svg", RegexOptions.IgnoreCase, RegexTimeout);
        public bool Validate(string url)
        {
            try
            {
                return GitHubBadgeUrlRegEx.IsMatch(url);
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }
    }
}
