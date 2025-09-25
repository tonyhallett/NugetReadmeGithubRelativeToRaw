using System;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using AngleSharp.Dom;

namespace NugetReadmeGithubRelativeToRaw
{
    internal class RewritableMarkdownElementsProvider : IRewritableMarkdownElementsProvider
    {
        public RelevantMarkdownElements GetRelevantMarkdownElementsWithSourceLocation(string readme, bool excludeHtml)
        {
            var pipeline = new MarkdownPipelineBuilder()
            .UsePreciseSourceLocation()
            .Build();
            var document = Markdown.Parse(readme, pipeline);
            return new RelevantMarkdownElements(
                document.Descendants<LinkInline>(), 
                excludeHtml ? Array.Empty<HtmlBlock>() : document.Descendants<HtmlBlock>(),
                excludeHtml ? Array.Empty<HtmlInline>() : document.Descendants<HtmlInline>()
                );
        }
    }
}
