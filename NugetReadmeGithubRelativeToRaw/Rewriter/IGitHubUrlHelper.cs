using System;

namespace NugetReadmeGithubRelativeToRaw.Rewriter
{
    interface IGitHubUrlHelper
    {
        string? GetGithubAbsoluteUrl(string? url, string rawUrl);

        Uri? GetAbsoluteUri(string? url);

        string GetAbsoluteOrGithubAbsoluteUrl(string url, string rawUrl);
    }
}
