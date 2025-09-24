using System;
using System.Text.RegularExpressions;

namespace NugetReadmeGithubRelativeToRaw
{
    internal class ReadmeRewriter
    {
        public string? Rewrite(string readme, string githubRepoUrl, string? repoBranch)
        {
            repoBranch = repoBranch ?? "master";
            string? rawUrl = null;

            if (!string.IsNullOrEmpty(githubRepoUrl))
            {
                var repoUrl = githubRepoUrl.TrimEnd('/');
                if (repoUrl.EndsWith(".git", StringComparison.OrdinalIgnoreCase))
                    repoUrl = repoUrl.Substring(0, repoUrl.Length - 4);

                if (repoUrl.StartsWith("https://github.com/", StringComparison.OrdinalIgnoreCase))
                {
                    var parts = repoUrl.Substring("https://github.com/".Length).Split('/');
                    if (parts.Length >= 2)
                    {
                        rawUrl = $"https://raw.githubusercontent.com/{parts[0]}/{parts[1]}/{repoBranch}";
                    }
                }
            }

            if (rawUrl == null)
            {
                return null;
            }

            return Regex.Replace(
                readme,
                @"(!?\[[^\]]*\]\()((?!https?:\/\/)[^)]+)(\))",
                m => $"{m.Groups[1].Value}{rawUrl}/{m.Groups[2].Value.TrimStart('/')}{m.Groups[3].Value}",
                RegexOptions.Compiled);
        }
    }
}
