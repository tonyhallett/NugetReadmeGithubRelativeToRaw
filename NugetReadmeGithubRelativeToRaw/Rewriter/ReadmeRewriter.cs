using System.Collections.Generic;
using System.Linq;
using NugetReadmeGithubRelativeToRaw.Rewriter.Validation;

namespace NugetReadmeGithubRelativeToRaw.Rewriter
{
    internal class ReadmeRewriter : IReadmeRewriter
    {
        internal const string GithubReadmeMarker = "{githubreadme_marker}";
        private readonly IRewritableMarkdownElementsProvider rewritableMarkdownElementsProvider;
        private readonly IReadmeReplacer readmeReplacer;
        private readonly IReadmeMarkdownElementsProcessor readmeMarkdownElementsProcessor;
        private readonly IRemoveReplacer removeReplace;
        private readonly IGitHubUrlHelper gitHubUrlHelper;

        internal ReadmeRewriter(
            IRewritableMarkdownElementsProvider rewritableMarkdownElementsProvider,
            IReadmeReplacer readmeReplacer,
            IReadmeMarkdownElementsProcessor readmeMarkdownElementsProcessor,
            IRemoveReplacer removeReplace,
            IGitHubUrlHelper gitHubUrlHelper
            )
        {
            this.rewritableMarkdownElementsProvider = rewritableMarkdownElementsProvider;
            this.readmeReplacer = readmeReplacer;
            this.readmeMarkdownElementsProcessor = readmeMarkdownElementsProcessor;
            this.removeReplace = removeReplace;
            this.gitHubUrlHelper = gitHubUrlHelper;
        }

        public ReadmeRewriter() : this(
            new RewritableMarkdownElementsProvider(),
            new ReadmeReplacer(),
            new ReadmeMarkdownElementsProcessor(
                new NuGetImageDomainValidator(NuGetTrustedImageDomains.Instance, new NuGetGitHubBadgeValidator()),
                GitHubUrlHelper.Instance,
                new HtmlFragmentParser()
                ),
            new RemoveReplacer(new RemoveReplaceRegexesFactory()),
            GitHubUrlHelper.Instance
            )
        {
        }

        // the githubRef is the branch, tag or commit sha, if null the master branch is used
        public ReadmeRewriterResult Rewrite(
            RewriteTagsOptions rewriteTagsOptions,
            string readme,
            string readmeRelativePath,
            string githubRepoUrl,
            string? githubRef = null,
            RemoveReplaceSettings? removeReplaceSettings = null)
        {
            OwnerRepoRefReadmePath? ownerRepoRefReadmePath = OwnerRepoRefReadmePath.Create(githubRepoUrl, githubRef, readmeRelativePath);
            
            if (removeReplaceSettings != null)
            {
                ApplyGithubReplacementText(removeReplaceSettings.RemovalsOrReplacements, ownerRepoRefReadmePath, readmeRelativePath);
                readme = removeReplace.RemoveReplace(readme, removeReplaceSettings);
            }

            return Rewrite(readme, ownerRepoRefReadmePath, rewriteTagsOptions);
        }

        private void ApplyGithubReplacementText(List<RemovalOrReplacement> removalsOrReplacements, OwnerRepoRefReadmePath? ownerRepoRefReadmePath, string readmeRelativePath)
        {
            if (ownerRepoRefReadmePath != null)
            {
                removalsOrReplacements.Where(removalOrReplacement => removalOrReplacement.ReplacementText != null && removalOrReplacement.ReplacementText.Contains(GithubReadmeMarker)).ToList().ForEach(replacement =>
                {
                    var url = gitHubUrlHelper.GetAbsoluteOrGitHubAbsoluteUrl(readmeRelativePath, ownerRepoRefReadmePath, false);
                    replacement.ReplacementText = replacement.ReplacementText!.Replace(GithubReadmeMarker, url);
                });
            }
        }

        private ReadmeRewriterResult Rewrite(string readme, OwnerRepoRefReadmePath? ownerRepoRefReadmePath, RewriteTagsOptions rewriteTagsOptions)
        {
            var unsupportedRepo = ownerRepoRefReadmePath == null;
            var relevantMarkdownElements = rewritableMarkdownElementsProvider.GetRelevantMarkdownElementsWithSourceLocation(readme, rewriteTagsOptions == RewriteTagsOptions.None);
            var hasUnsupportedHTML = false;
            if(rewriteTagsOptions.HasFlag(RewriteTagsOptions.Error))
            {
                hasUnsupportedHTML = relevantMarkdownElements.HtmlInlines.Any() | relevantMarkdownElements.HtmlBlocks.Any();
                relevantMarkdownElements.RemoveHTML();
            }

            var markdownElementsProcessResult = readmeMarkdownElementsProcessor.Process(relevantMarkdownElements, ownerRepoRefReadmePath, rewriteTagsOptions);
            string? rewrittenReadme = null;
            if (!hasUnsupportedHTML && !markdownElementsProcessResult.UnsupportedImageDomains.Any() && !unsupportedRepo)
            {
                rewrittenReadme = readmeReplacer.Replace(readme, markdownElementsProcessResult.SourceReplacements);
            }
            
            return new ReadmeRewriterResult(rewrittenReadme, markdownElementsProcessResult.UnsupportedImageDomains, hasUnsupportedHTML, unsupportedRepo);
        }
    }
}
