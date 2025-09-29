using NugetReadmeGithubRelativeToRaw.Rewriter.Validation;

namespace NugetReadmeGithubRelativeToRaw.Rewriter
{
    internal class ReadmeRewriter
    {
        private readonly IRewritableMarkdownElementsProvider rewritableMarkdownElementsProvider;
        private readonly IReadmeReplacer readmeReplacer;
        private readonly IReadmeMarkdownElementsProcessor readmeMarkdownElementsProcessor;
        private readonly IRemoveReplacer removeReplace;

        internal ReadmeRewriter(
            IRewritableMarkdownElementsProvider rewritableMarkdownElementsProvider,
            IReadmeReplacer readmeReplacer,
            IReadmeMarkdownElementsProcessor readmeMarkdownElementsProcessor,
            IRemoveReplacer removeReplace
            )
        {
            this.rewritableMarkdownElementsProvider = rewritableMarkdownElementsProvider;
            this.readmeReplacer = readmeReplacer;
            this.readmeMarkdownElementsProcessor = readmeMarkdownElementsProcessor;
            this.removeReplace = removeReplace;
        }

        public ReadmeRewriter() : this(
            new RewritableMarkdownElementsProvider(),
            new ReadmeReplacer(),
            new ReadmeMarkdownElementsProcessor(
                new NuGetImageDomainValidator(NuGetTrustedImageDomains.Instance, new NuGetGitHubBadgeValidator()),
                new GitHubUrlHelper(),
                new HtmlFragmentParser()
                ),
            new RemoveReplacer(new RemoveReplaceRegexesFactory())
            )
        {
        }

        // the githubRef is the branch, tag or commit sha, if null the master branch is used
        public ReadmeRewriterResult? Rewrite(
            string readme, 
            string readmeRelativePath,
            string githubRepoUrl, 
            string? githubRef = null, 
            RewriteTagsOptions  rewriteTagsOptions = RewriteTagsOptions.All,
            RemoveReplaceSettings? removeReplaceSettings = null)
        {
            readme = removeReplaceSettings == null ? readme : removeReplace.RemoveReplace(readme, removeReplaceSettings);
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
