namespace NugetReadmeGithubRelativeToRaw.Rewriter
{
    internal interface IReadmeMarkdownElementsProcessor {
        IMarkdownElementsProcessResult Process(
            RelevantMarkdownElements relevantMarkdownElements, 
            RepoPaths? repoPaths, 
            RewriteTagsOptions rewriteTagsOptions,
            IReadmeRelativeFileExists readmeRelativeFileExists);
    }
}
