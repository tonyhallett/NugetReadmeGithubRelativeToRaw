using NugetReadmeGithubRelativeToRaw.MSBuildHelpers;

namespace NugetReadmeGithubRelativeToRaw
{
    internal class RemoveReplaceMetadata : RequiredMetadataBase
    {
        public string? ReplacementText { get; set; }

        [RequiredMetadata]
        public string? CommentOrRegex { get; set; }

        [RequiredMetadata]
        public string? Start { get; set; }
        
        public string? End { get; set; }
    }
}
