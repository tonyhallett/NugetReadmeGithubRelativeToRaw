using System;
using NugetReadmeGithubRelativeToRaw.Rewriter.Validation;

namespace NugetReadmeGithubRelativeToRaw.Rewriter
{
    internal class ReadmeRewriter
    {
        private readonly IRewritableMarkdownElementsProvider rewritableMarkdownElementsProvider;
        private readonly IReadmeReplacer readmeReplacer;
        private readonly IReadmeMarkdownElementsProcessor readmeMarkdownElementsProcessor;

        internal ReadmeRewriter(
            IRewritableMarkdownElementsProvider rewritableMarkdownElementsProvider,
            IReadmeReplacer readmeReplacer,
            IReadmeMarkdownElementsProcessor readmeMarkdownElementsProcessor
            )
        {
            this.rewritableMarkdownElementsProvider = rewritableMarkdownElementsProvider;
            this.readmeReplacer = readmeReplacer;
            this.readmeMarkdownElementsProcessor = readmeMarkdownElementsProcessor;
        }

        public ReadmeRewriter() : this(
            new RewritableMarkdownElementsProvider(),
            new ReadmeReplacer(),
            new ReadmeMarkdownElementsProcessor(
                new NugetImageDomainValidator(NugetTrustedImageDomains.Instance, new NugetGitHubBadgeValidator()),
                new GitHubUrlHelper(),
                new HtmlFragmentParser()
                )
            )
        {
        }

        public ReadmeRewriterResult? Rewrite(string readme, string githubRepoUrl, string? repoBranch, RewriteTagsOptions  rewriteTagsOptions = RewriteTagsOptions.All)
        {
            string? rawUrl = GetRawUrl(githubRepoUrl, repoBranch);
            return rawUrl == null ? null : Rewrite(readme, rawUrl, rewriteTagsOptions);
        }

        private string? GetRawUrl(string githubRepoUrl, string? repoBranch)
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

            return rawUrl;
        }

        
        private ReadmeRewriterResult Rewrite(string readme,string rawUrl, RewriteTagsOptions rewriteTagsOptions)
        {
            var relevantMarkdownElements = rewritableMarkdownElementsProvider.GetRelevantMarkdownElementsWithSourceLocation(readme, rewriteTagsOptions == RewriteTagsOptions.None);
            
            var markdownElementsProcessResult = readmeMarkdownElementsProcessor.Process(relevantMarkdownElements, rawUrl, rewriteTagsOptions);
            
            var rewrittenReadme = readmeReplacer.Replace(readme, markdownElementsProcessResult.SourceReplacements);
            return new ReadmeRewriterResult(rewrittenReadme, markdownElementsProcessResult.UnsupportedImageDomains);
        }






    }
}
