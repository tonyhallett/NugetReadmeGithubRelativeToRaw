using System.Text.RegularExpressions;

namespace NugetReadmeGithubRelativeToRaw.Rewriter
{
    internal class RemoveCommentRegexes
    {
        public RemoveCommentRegexes(Regex startRegex, Regex endRegex)
        {
            StartRegex = startRegex;
            EndRegex = endRegex;
        }
        public Regex StartRegex { get; }

        public Regex EndRegex { get; }
        
        public static RemoveCommentRegexes Create(RemoveCommentIdentifiers removeCommentIdentifiers)
        {
            return new RemoveCommentRegexes(
                CreateRegex(removeCommentIdentifiers.Start),
                CreateRegex(removeCommentIdentifiers.End));
        }

        public static Regex CreateRegex(string commentIdentifier)
        {
            var startPattern = @"<!--\s*" + Regex.Escape(commentIdentifier) + @"\b[^>]*-->";
            return new Regex(startPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }
    }
}
