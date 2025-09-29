using System.Collections.Generic;

namespace NugetReadmeGithubRelativeToRaw.Rewriter
{
    internal class RemoveReplaceSettings
    {
        public RemoveReplaceSettings(
            RemoveCommentIdentifiers? removeCommentIdentifiers, 
            List<RemovalOrReplacement> removalsOrReplacements)
        {
            RemoveCommentIdentifiers = removeCommentIdentifiers;
            RemovalsOrReplacements = removalsOrReplacements;
        }

        public RemoveCommentIdentifiers? RemoveCommentIdentifiers { get; }
        public List<RemovalOrReplacement> RemovalsOrReplacements { get; }
    }
}
