namespace NugetReadmeGithubRelativeToRaw.Rewriter
{
    internal interface IReadmeRewriter
    {
        ReadmeRewriterResult? Rewrite(string readme, string readmeRelativePath, string githubRepoUrl, string? githubRef = null, RewriteTagsOptions rewriteTagsOptions = RewriteTagsOptions.All, RemoveReplaceSettings? removeReplaceSettings = null);
    }
}