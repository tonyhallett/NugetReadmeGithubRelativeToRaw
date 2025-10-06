using AngleSharp.Dom;
using Markdig.Syntax;

namespace NugetReadmeGithubRelativeToRaw.Rewriter
{
    internal interface IHtmlFragmentParser
    {
        T Parse<T>(HtmlBlock htmlBlock) where T : INode;

        INode Parse(string fragment);
    }
}