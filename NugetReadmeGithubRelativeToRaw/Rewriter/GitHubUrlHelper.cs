using System;

namespace NugetReadmeGithubRelativeToRaw.Rewriter
{
    internal class GitHubUrlHelper : IGitHubUrlHelper
    {
        public static GitHubUrlHelper Instance { get; } = new GitHubUrlHelper();

        public string? GetGitHubAbsoluteUrl(
            string? url, 
            OwnerRepoRefReadmePath ownerRepoRefReadmePath, 
            bool isImage)
        {
            if (url == null || IsAbsolute(url))
            {
                return null;
            }

            url = url.Trim();

            string urlWithoutPath = isImage ? $"https://raw.githubusercontent.com/{ownerRepoRefReadmePath.OwnerRepoUrlPart}/{ownerRepoRefReadmePath.Ref}" :
                $"https://github.com/{ownerRepoRefReadmePath.OwnerRepoUrlPart}/blob/{ownerRepoRefReadmePath.Ref}";

            // repo relative
            if (url.StartsWith("/"))
            {
                return $"{urlWithoutPath}{url}";
            }

            // readme directory relative
            var readmeRelativePath = ownerRepoRefReadmePath.ReadmeRelativePath;
            if (!readmeRelativePath.StartsWith("/"))
            {
                readmeRelativePath = "/" + readmeRelativePath;
            }

            var readmeUri = new Uri(urlWithoutPath + readmeRelativePath);
            return new Uri(readmeUri, url).OriginalString;
        }

        public string GetAbsoluteOrGitHubAbsoluteUrl(string url, OwnerRepoRefReadmePath ownerRepoRefReadmePath, bool isImage)
        {
            if (IsAbsolute(url))
            {
                return url!;
            }

            return GetGitHubAbsoluteUrl(url, ownerRepoRefReadmePath, isImage)!;
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
