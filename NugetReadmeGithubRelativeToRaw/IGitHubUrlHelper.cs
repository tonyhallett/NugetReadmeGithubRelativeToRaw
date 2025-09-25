using System;

namespace NugetReadmeGithubRelativeToRaw
{
    interface IGitHubUrlHelper
    {
        string? GetGithubAbsoluteUrl(string? url, string rawUrl);
        Uri? GetAbsoluteUri(string? url);
    }
}
