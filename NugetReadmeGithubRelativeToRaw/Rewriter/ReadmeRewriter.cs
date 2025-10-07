using System.Collections.Generic;
using System.Linq;
using NugetReadmeGithubRelativeToRaw.Rewriter.Validation;

namespace NugetReadmeGithubRelativeToRaw.Rewriter
{
    internal class ReadmeRewriter : IReadmeRewriter
    {
        internal const string ReadmeMarker = "{readme_marker}";
        private readonly IRewritableMarkdownElementsProvider _rewritableMarkdownElementsProvider;
        private readonly IReadmeReplacer _readmeReplacer;
        private readonly IReadmeMarkdownElementsProcessor _readmeMarkdownElementsProcessor;
        private readonly IRemoveReplacer _removeReplacer;
        private readonly IRepoUrlHelper _repoUrlHelper;

        internal ReadmeRewriter(
            IRewritableMarkdownElementsProvider rewritableMarkdownElementsProvider,
            IReadmeReplacer readmeReplacer,
            IReadmeMarkdownElementsProcessor readmeMarkdownElementsProcessor,
            IRemoveReplacer removeReplace,
            IRepoUrlHelper repoUrlHelper
            )
        {
            _rewritableMarkdownElementsProvider = rewritableMarkdownElementsProvider;
            _readmeReplacer = readmeReplacer;
            _readmeMarkdownElementsProcessor = readmeMarkdownElementsProcessor;
            _removeReplacer = removeReplace;
            _repoUrlHelper = repoUrlHelper;
        }

        public ReadmeRewriter() : this(
            new RewritableMarkdownElementsProvider(),
            new ReadmeReplacer(),
            new ReadmeMarkdownElementsProcessor(
                new NuGetImageDomainValidator(NuGetTrustedImageDomains.Instance, new NuGetGitHubBadgeValidator()),
                RepoUrlHelper.Instance,
                new HtmlFragmentParser()
                ),
            new RemoveReplacer(new RemoveReplaceRegexesFactory()),
            RepoUrlHelper.Instance
            )
        {
        }

        // the ref is the branch, tag or commit sha
        public ReadmeRewriterResult Rewrite(
            RewriteTagsOptions rewriteTagsOptions,
            string readme,
            string readmeRelativePath,
            string? repoUrl,
            string @ref,
            RemoveReplaceSettings? removeReplaceSettings,
            IReadmeRelativeFileExists readmeRelativeFileExists
            )
        {
            RepoPaths? repoPaths = repoUrl != null ? RepoPaths.Create(repoUrl, @ref, readmeRelativePath) : null;
            
            if (removeReplaceSettings != null)
            {
                ApplyRepoReadmeReplacementText(removeReplaceSettings.RemovalsOrReplacements, repoPaths, readmeRelativePath);
                readme = _removeReplacer.RemoveReplace(readme, removeReplaceSettings);
            }

            return Rewrite(readme, repoPaths, rewriteTagsOptions, readmeRelativeFileExists);
        }

        private void ApplyRepoReadmeReplacementText(List<RemovalOrReplacement> removalsOrReplacements, RepoPaths? repoPaths, string readmeRelativePath)
        {
            if (repoPaths != null)
            {
                removalsOrReplacements.Where(removalOrReplacement => removalOrReplacement.ReplacementText != null && removalOrReplacement.ReplacementText.Contains(ReadmeMarker)).ToList().ForEach(replacement =>
                {
                    var url = _repoUrlHelper.GetAbsoluteOrRepoAbsoluteUrl(readmeRelativePath, repoPaths, false);
                    replacement.ReplacementText = replacement.ReplacementText!.Replace(ReadmeMarker, url);
                });
            }
        }

        private ReadmeRewriterResult Rewrite(string readme, RepoPaths? repoPaths, RewriteTagsOptions rewriteTagsOptions, IReadmeRelativeFileExists readmeRelativeFileExists)
        {
            var unsupportedRepo = repoPaths == null;
            var relevantMarkdownElements = _rewritableMarkdownElementsProvider.GetRelevantMarkdownElementsWithSourceLocation(readme, rewriteTagsOptions == RewriteTagsOptions.None);
            var hasUnsupportedHTML = false;
            if (rewriteTagsOptions.HasFlag(RewriteTagsOptions.Error))
            {
                hasUnsupportedHTML = relevantMarkdownElements.HtmlInlines.Any() | relevantMarkdownElements.HtmlBlocks.Any();
                relevantMarkdownElements.RemoveHTML();
            }

            var markdownElementsProcessResult = _readmeMarkdownElementsProcessor.Process(relevantMarkdownElements, repoPaths, rewriteTagsOptions, readmeRelativeFileExists);
            string? rewrittenReadme = null;
            if (!hasUnsupportedHTML && !markdownElementsProcessResult.HasErrors() && !unsupportedRepo)
            {
                rewrittenReadme = _readmeReplacer.Replace(readme, markdownElementsProcessResult.SourceReplacements);
            }
            
            return new ReadmeRewriterResult(rewrittenReadme, markdownElementsProcessResult.UnsupportedImageDomains,markdownElementsProcessResult.MissingReadmeAssets, hasUnsupportedHTML, unsupportedRepo);
        }
    }
}
