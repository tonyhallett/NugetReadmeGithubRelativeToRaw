namespace NugetReadmeGithubRelativeToRaw.Rewriter
{
    internal interface IReadmeRewriter
    {
        ReadmeRewriterResult Rewrite(
            RewriteTagsOptions rewriteTagsOptions,
            string readme, 
            string readmeRelativePath, 
            string githubRepoUrl, 
            string? githubRef = null, 
            RemoveReplaceSettings? removeReplaceSettings = null);
    }
}