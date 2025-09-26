using System;

namespace NugetReadmeGithubRelativeToRaw.Rewriter
{
    interface IGitHubUrlHelper
    {
        string? GetGithubAbsoluteUrl(string? url, OwnerRepoRefReadmePath ownerRepoRefReadmePath, bool isImage);

        Uri? GetAbsoluteUri(string? url);

        string GetAbsoluteOrGithubAbsoluteUrl(string url, OwnerRepoRefReadmePath ownerRepoRefReadmePath, bool isImage);
    }
}
