using System.Collections.Generic;
using System.Linq;
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
        public IEnumerable<HtmlBlock> HtmlBlocks { get; private set; }
        public IEnumerable<HtmlInline> HtmlInlines { get; private set; }

        public void RemoveHTML()
        {
            HtmlBlocks = Enumerable.Empty<HtmlBlock>();
            HtmlInlines = Enumerable.Empty<HtmlInline>();
        }
    }
}
