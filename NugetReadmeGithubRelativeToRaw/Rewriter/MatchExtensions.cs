using System.Text.RegularExpressions;

namespace NugetReadmeGithubRelativeToRaw.Rewriter
{
    internal static class MatchExtensions
    {
        public static string Before(this Match match, string text)
            => text.Substring(0, match.Index);

        public static string After(this Match match, string text) => text.Substring(match.Index + match.Length);
    }
}
