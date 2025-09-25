using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace NugetReadmeGithubRelativeToRaw
{
    internal class ReadmeRewriterResult
    {
        public ReadmeRewriterResult(string rewrittenReadme, IEnumerable<string> unsupportedImageDomains)
        {
            RewrittenReadme = rewrittenReadme;
            UnsupportedImageDomains = new List<string>(unsupportedImageDomains);
        }

        public string RewrittenReadme { get; }

        public IReadOnlyList<string> UnsupportedImageDomains { get; }
    }

    [Flags]
    internal enum RewriteTagsOptions
    {
        RewriteImgTagsForSupportedDomains = 1,
        RewriteATags = 2,
        RewriteBrTags = 3,
        All = RewriteImgTagsForSupportedDomains | RewriteATags | RewriteBrTags
    }

    internal class ReadmeRewriter
    {
        private readonly INugetImageDomainValidator nugetImageDomainValidator;

        public ReadmeRewriter(INugetImageDomainValidator nugetImageDomainValidator)
        {
            this.nugetImageDomainValidator = nugetImageDomainValidator;
        }

        public ReadmeRewriter() : this(new NugetImageDomainValidator(NugetTrustedImageDomains.Instance))
        {
        }

        public ReadmeRewriterResult? Rewrite(string readme, string githubRepoUrl, string? repoBranch, RewriteTagsOptions  rewriteTagsOptions = RewriteTagsOptions.All)
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

            

            return Rewrite(readme, rawUrl, rewriteTagsOptions);
        }

        private ReadmeRewriterResult Rewrite(string readme,string rawUrl, RewriteTagsOptions rewriteTagsOptions)
        {
            var tempRegexSolution =  Regex.Replace(
                readme,
                @"(!?\[[^\]]*\]\()((?!https?:\/\/)[^)]+)(\))",
                m => $"{m.Groups[1].Value}{rawUrl}/{m.Groups[2].Value.TrimStart('/')}{m.Groups[3].Value}",
                RegexOptions.Compiled);
            return new ReadmeRewriterResult(tempRegexSolution, new string[0]);
        }
    }
}
