using System.Linq;
using AngleSharp.Html.Parser;
using AngleSharp;
using Markdig.Syntax;
using AngleSharp.Dom;

namespace NugetReadmeGithubRelativeToRaw.Rewriter
{
    internal class HtmlFragmentParser : IHtmlFragmentParser
    {
        public T Parse<T>(HtmlBlock htmlBlock) where T : INode
        {
            var node =  Parse(htmlBlock.Lines.ToString());
            return (T)node;
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
