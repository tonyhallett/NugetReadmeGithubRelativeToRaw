using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace NugetReadmeGithubRelativeToRaw.Rewriter.Validation
{
    [ExcludeFromCodeCoverage]
    internal class NugetGitHubBadgeValidator : INugetGitHubBadgeValidator
    {
        private static readonly TimeSpan RegexTimeout = TimeSpan.FromMinutes(1);
        private static readonly Regex GithubBadgeUrlRegEx = new Regex("^(https|http):\\/\\/github\\.com\\/[^/]+\\/[^/]+(\\/actions)?\\/workflows\\/.*badge\\.svg", RegexOptions.IgnoreCase, RegexTimeout);
        public bool Validate(string url)
        {
            try
            {
                return GithubBadgeUrlRegEx.IsMatch(url);
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }
    }
}
