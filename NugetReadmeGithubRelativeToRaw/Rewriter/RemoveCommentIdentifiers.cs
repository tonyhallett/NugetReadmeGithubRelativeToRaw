namespace NugetReadmeGithubRelativeToRaw.Rewriter
{
    internal class RemoveCommentIdentifiers
    {
        public RemoveCommentIdentifiers(string startCommentIdentifier, string endCommentIdentifier)
        {
            StartCommentIdentifier = startCommentIdentifier;
            EndCommentIdentifier = endCommentIdentifier;
        }
        public string StartCommentIdentifier { get; }
        public string EndCommentIdentifier { get; }
    }
}
