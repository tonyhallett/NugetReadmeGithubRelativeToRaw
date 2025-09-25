using System;

namespace NugetReadmeGithubRelativeToRaw
{
    internal class GitHubUrlHelper : IGitHubUrlHelper
    {
        // todo when relative is relative to the readme and relative to the repository
        public string? GetGithubAbsoluteUrl(string? url, string rawUrl)
        {
            if (url == null || IsAbsolute(url))
            {
                return null;
            }

            // bool relativeToRepositorRoot = false;  todo

            return $"{rawUrl}/{url}";

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
