namespace NugetReadmeGithubRelativeToRaw.Rewriter
{
    internal interface IReadmeMarkdownElementsProcessor {
        
        IMarkdownElementsProcessResult Process(
            RelevantMarkdownElements relevantMarkdownElements, 
            OwnerRepoRefReadmePath? ownerRepoRefReadmePath, 
            RewriteTagsOptions rewriteTagsOptions);
    
    }
}
