using System.Collections.Generic;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace NugetReadmeGithubRelativeToRaw.Rewriter
{
    internal class RelevantMarkdownElements
    {
        public RelevantMarkdownElements(
            IEnumerable<LinkInline> linkInlines, 
            IEnumerable<HtmlBlock> htmlBlocks,
            IEnumerable<HtmlInline> htmlInlines)
        {
            LinkInlines = linkInlines;
            HtmlBlocks = htmlBlocks;
            HtmlInlines = htmlInlines;
        }

        public IEnumerable<LinkInline> LinkInlines { get; }
        public IEnumerable<HtmlBlock> HtmlBlocks { get; }
        public IEnumerable<HtmlInline> HtmlInlines { get; }
    }
}
