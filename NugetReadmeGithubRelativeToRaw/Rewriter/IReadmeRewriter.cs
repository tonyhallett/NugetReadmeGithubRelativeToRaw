namespace NugetReadmeGithubRelativeToRaw.Rewriter
{
    internal interface IReadmeRewriter
    {
        ReadmeRewriterResult Rewrite(
            RewriteTagsOptions rewriteTagsOptions,
            string readme, 
            string readmeRelativePath, 
            string? repoUrl, 
            string? @ref, 
            RemoveReplaceSettings? removeReplaceSettings,
            IReadmeRelativeFileExists readmeRelativeFileExists);
    }
}