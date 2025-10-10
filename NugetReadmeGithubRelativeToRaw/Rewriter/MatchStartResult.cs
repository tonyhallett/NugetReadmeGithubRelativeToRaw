namespace NugetReadmeGithubRelativeToRaw.Rewriter
{
    internal class MatchStartResult
    {
        public MatchStartResult(System.Text.RegularExpressions.Match match, bool isRemaining = false, string? replacementText = null)
        {
            Match = match;
            IsRemaining = isRemaining;
            ReplacementText = replacementText;
        }
        public System.Text.RegularExpressions.Match Match { get; }

        public bool IsRemaining { get; }

        public string? ReplacementText { get; }
    }
}
