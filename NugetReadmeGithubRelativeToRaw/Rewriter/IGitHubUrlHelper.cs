using System;

namespace NugetReadmeGithubRelativeToRaw.Rewriter
{
    interface IGitHubUrlHelper
    {
        string? GetGitHubAbsoluteUrl(string? url, OwnerRepoRefReadmePath ownerRepoRefReadmePath, bool isImage);

        Uri? GetAbsoluteUri(string? url);

        string GetAbsoluteOrGitHubAbsoluteUrl(string url, OwnerRepoRefReadmePath ownerRepoRefReadmePath, bool isImage);
    }
}
