using System.Linq;
using AngleSharp.Html.Parser;
using AngleSharp;
using Markdig.Syntax;
using AngleSharp.Dom;
using Markdig.Syntax.Inlines;

namespace NugetReadmeGithubRelativeToRaw
{
    internal static class HtmlBlockTransformer
    {
        public static INode TransformToDom(HtmlBlock htmlBlock)
        {
            var context = BrowsingContext.New(Configuration.Default);
            var parser = context.GetService<IHtmlParser>();
            var document = context.OpenNewAsync().Result;
            var body = document.Body;

            var htmlText = htmlBlock.Lines.ToString();
            var root = parser!.ParseFragment(htmlText, body!).First();
            return root;
        }
    }
}
