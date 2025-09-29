using System;

namespace NugetReadmeGithubRelativeToRaw.Rewriter
{
    internal class OwnerRepoRefReadmePath
    {
        public string OwnerRepoUrlPart { get; }
        public string Ref { get; }
        public string ReadmeRelativePath { get; }
        private OwnerRepoRefReadmePath(string ownerRepoUrlPart, string @ref, string readmeRelativePath)
        {
            OwnerRepoUrlPart = ownerRepoUrlPart;
            Ref = @ref;
            ReadmeRelativePath = readmeRelativePath;
        }

        public static OwnerRepoRefReadmePath? Create(string gitHubRepoUrl, string? githubRef, string readMeRelativePath)
        {
            gitHubRepoUrl = GetGitHubRepoUrl(gitHubRepoUrl);

            if (gitHubRepoUrl.StartsWith("https://github.com/", StringComparison.OrdinalIgnoreCase))
            {
                var parts = gitHubRepoUrl.Substring("https://github.com/".Length).Split('/');
                if (parts.Length >= 2)
                {
                    var ownerRepoRefUrlPart = $"{parts[0]}/{parts[1]}";
                    return new OwnerRepoRefReadmePath(ownerRepoRefUrlPart, githubRef ?? "master", readMeRelativePath);
                }
            }
            return null;
        }

        private static string GetGitHubRepoUrl(string githubRepoUrl)
        {
            var repoUrl = githubRepoUrl.TrimEnd('/');
            if (repoUrl.EndsWith(".git", StringComparison.OrdinalIgnoreCase))
            {
                repoUrl = repoUrl.Substring(0, repoUrl.Length - 4);
            }

            return repoUrl;
        }
    }
}
