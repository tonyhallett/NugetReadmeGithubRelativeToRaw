using System.Linq;
using Markdig.Syntax;

namespace NugetReadmeGithubRelativeToRaw.Rewriter
{
    internal static class SourceSpanExtensions
    {
        public static SourceSpan Combine(this SourceSpan first, params SourceSpan[] otherSpans)
            => new SourceSpan(first.Start, first.End + otherSpans.Sum(other => other.Length));
    }
}
