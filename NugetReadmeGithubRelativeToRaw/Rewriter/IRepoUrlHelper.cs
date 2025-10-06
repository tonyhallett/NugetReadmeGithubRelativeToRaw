using System;

namespace NugetReadmeGithubRelativeToRaw.Rewriter
{
    interface IRepoUrlHelper
    {
        string? GetRepoAbsoluteUrl(string? url, RepoPaths ownerRepoRefReadmePath, bool isImage);

        Uri? GetAbsoluteUri(string? url);

        string GetAbsoluteOrRepoAbsoluteUrl(string url, RepoPaths ownerRepoRefReadmePath, bool isImage);
    }
}
