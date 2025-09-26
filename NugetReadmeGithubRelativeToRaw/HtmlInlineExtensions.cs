using Markdig.Syntax.Inlines;
using System.Text.RegularExpressions;

namespace NugetReadmeGithubRelativeToRaw
{
    internal static class HtmlInlineExtensions
    {
        public static bool IsBrTag(this HtmlInline htmlInline)
        {
            var tag = htmlInline.Tag.Trim();

            // Match <br>, <br/>, <br />, <br    />, etc.
            return Regex.IsMatch(tag, @"^<br\s*/?>$", RegexOptions.IgnoreCase);
        }


    }
}
