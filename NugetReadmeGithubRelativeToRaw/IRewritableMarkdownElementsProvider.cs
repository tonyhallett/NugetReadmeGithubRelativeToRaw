namespace NugetReadmeGithubRelativeToRaw
{
    interface IRewritableMarkdownElementsProvider {
        RelevantMarkdownElements GetRelevantMarkdownElementsWithSourceLocation(
            string readme, 
            bool excludeHtml);
    }
}
