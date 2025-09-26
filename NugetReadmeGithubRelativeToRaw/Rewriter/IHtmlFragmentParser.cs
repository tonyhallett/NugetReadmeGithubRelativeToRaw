using AngleSharp.Dom;
using Markdig.Syntax;

namespace NugetReadmeGithubRelativeToRaw.Rewriter
{
    internal interface IHtmlFragmentParser
    {
        INode Parse(HtmlBlock htmlBlock);
        INode Parse(string fragment);
    }
}