using System;

namespace NugetReadmeGithubRelativeToRaw
{
    internal interface INugetGitHubBadgeValidator
    {
        bool Validate(string url);
    }
}
