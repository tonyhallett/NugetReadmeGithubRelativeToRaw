using System;

namespace NugetReadmeGithubRelativeToRaw.Rewriter
{
    internal class GitHubUrlHelper : IGitHubUrlHelper
    {
        public string? GetGithubAbsoluteUrl(
            string? url, 
            OwnerRepoRefReadmePath ownerRepoRefReadmePath, 
            bool isImage)
        {
            if (url == null || IsAbsolute(url))
            {
                return null;
            }

            url = url.Trim();

            // ignore empty and fragments
            if(string.IsNullOrEmpty(url) || url.StartsWith("#", StringComparison.Ordinal))
            {
                return null;
            }

            string urlWithoutPath = isImage ? $"https://raw.githubusercontent.com/{ownerRepoRefReadmePath.OwnerRepoUrlPart}/{ownerRepoRefReadmePath.Ref}" :
                $"https://github.com/{ownerRepoRefReadmePath.OwnerRepoUrlPart}/blob/{ownerRepoRefReadmePath.Ref}";

            // todo

            return $"{urlWithoutPath}/{url}";
        }

        public string GetAbsoluteOrGithubAbsoluteUrl(string url, OwnerRepoRefReadmePath ownerRepoRefReadmePath, bool isImage)
        {
            if (IsAbsolute(url))
            {
                return url!;
            }

            return GetGithubAbsoluteUrl(url, ownerRepoRefReadmePath, isImage)!;
        }

        public Uri? GetAbsoluteUri(string? url)
        {
            if (url == null)
            {
                return null;
            }

            if (Uri.TryCreate(url, UriKind.Absolute, out var absoluteUri))
            {
                return absoluteUri;
            }
            return null;
        }

        private bool IsAbsolute(string? url) => GetAbsoluteUri(url) != null;
    }
}
