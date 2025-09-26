using AngleSharp.Dom;
using Markdig.Syntax;

namespace NugetReadmeGithubRelativeToRaw
{
    internal interface IHtmlFragmentParser
    {
        INode Parse(HtmlBlock htmlBlock);
        INode Parse(string fragment);
    }
}