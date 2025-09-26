namespace NugetReadmeGithubRelativeToRaw.Rewriter
{
    internal interface IReadmeMarkdownElementsProcessor {
        
        IMarkdownElementsProcessResult Process(RelevantMarkdownElements relevantMarkdownElements, string rawUrl, RewriteTagsOptions rewriteTagsOptions);
    
    }
}
