using System;
using System.Globalization;
using System.Security.AccessControl;
using NugetReadmeGithubRelativeToRaw.Rewriter.Validation;

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
                new NuGetImageDomainValidator(NuGetTrustedImageDomains.Instance, new NuGetGitHubBadgeValidator()),
                new GitHubUrlHelper(),
                new HtmlFragmentParser()
                )
            )
        {
        }

        // the githubRef is the branch, tag or commit sha, if null the master branch is used
        public ReadmeRewriterResult? Rewrite(
            string readme, 
            string readmeRelativePath,
            string githubRepoUrl, 
            string? githubRef = null, 
            RewriteTagsOptions  rewriteTagsOptions = RewriteTagsOptions.All)
        {
            OwnerRepoRefReadmePath? ownerRepoRefReadmePath = OwnerRepoRefReadmePath.Create(githubRepoUrl, githubRef, readmeRelativePath);
            
            return ownerRepoRefReadmePath == null ? null : Rewrite(readme, ownerRepoRefReadmePath, rewriteTagsOptions);
        }

        
        private ReadmeRewriterResult Rewrite(string readme,OwnerRepoRefReadmePath ownerRepoRefReadmePath, RewriteTagsOptions rewriteTagsOptions)
        {
            var relevantMarkdownElements = rewritableMarkdownElementsProvider.GetRelevantMarkdownElementsWithSourceLocation(readme, rewriteTagsOptions == RewriteTagsOptions.None);
            
            var markdownElementsProcessResult = readmeMarkdownElementsProcessor.Process(relevantMarkdownElements, ownerRepoRefReadmePath, rewriteTagsOptions);
            
            var rewrittenReadme = readmeReplacer.Replace(readme, markdownElementsProcessResult.SourceReplacements);
            return new ReadmeRewriterResult(rewrittenReadme, markdownElementsProcessResult.UnsupportedImageDomains);
        }






    }
}
