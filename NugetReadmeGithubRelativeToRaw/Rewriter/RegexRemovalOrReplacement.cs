using System.Text.RegularExpressions;

namespace NugetReadmeGithubRelativeToRaw.Rewriter
{
    internal class RegexRemovalOrReplacement
    {
        public RegexRemovalOrReplacement(Regex startRegex, Regex? endRegex, string? replacementText)
        {
            StartRegex = startRegex;
            EndRegex = endRegex;
            ReplacementText = replacementText;
        }
        public Regex StartRegex { get; }
        public Regex? EndRegex { get; }
        public string? ReplacementText { get; }

        private static Regex CreateRegex(CommentOrRegex commentOrRegex, string pattern)
        {
            return commentOrRegex == CommentOrRegex.Regex
                ? new Regex(pattern, RegexOptions.Compiled) // should add IgnoreCase ?
                : RemoveCommentRegexes.CreateRegex(pattern);
        }

        public static RegexRemovalOrReplacement Create(RemovalOrReplacement removalOrReplacement)
        {
            var startRegex = CreateRegex(removalOrReplacement.CommentOrRegex, removalOrReplacement.Start);
            Regex? endRegex = null;
            if(removalOrReplacement.End != null)
            {
                endRegex = CreateRegex(removalOrReplacement.CommentOrRegex, removalOrReplacement.End);
            }

            return new RegexRemovalOrReplacement(startRegex, endRegex, removalOrReplacement.ReplacementText);
        }
    }
}
