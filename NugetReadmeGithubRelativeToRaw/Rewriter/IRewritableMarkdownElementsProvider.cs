namespace NugetReadmeGithubRelativeToRaw.Rewriter
{
    interface IRewritableMarkdownElementsProvider {
        RelevantMarkdownElements GetRelevantMarkdownElementsWithSourceLocation(
            string readme, 
            bool excludeHtml);
    }
}
