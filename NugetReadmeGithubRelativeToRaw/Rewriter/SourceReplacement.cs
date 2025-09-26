using Markdig.Syntax;

namespace NugetReadmeGithubRelativeToRaw.Rewriter
{
    internal class SourceReplacement
    {
        public SourceReplacement(SourceSpan sourceSpan, string replacement)
        {
            Start = sourceSpan.Start;
            End = sourceSpan.End;
            Replacement = replacement;
        }

        public int Start { get; }
        public int End { get; }
        public string Replacement { get; }
    }
}
