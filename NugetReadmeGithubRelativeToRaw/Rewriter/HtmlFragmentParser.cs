using System.Linq;
using AngleSharp.Html.Parser;
using AngleSharp;
using Markdig.Syntax;
using AngleSharp.Dom;

namespace NugetReadmeGithubRelativeToRaw.Rewriter
{
    internal class HtmlFragmentParser : IHtmlFragmentParser
    {
        public INode Parse(HtmlBlock htmlBlock)
        {
            return Parse(htmlBlock.Lines.ToString());
        }

        public INode Parse(string fragment)
        {
            var context = BrowsingContext.New(Configuration.Default);
            var parser = context.GetService<IHtmlParser>();
            var document = context.OpenNewAsync().Result;
            var body = document.Body;
            var root = parser!.ParseFragment(fragment, body!).First();
            return root;
        }
    }
}
