using System.Linq;

namespace NugetReadmeGithubRelativeToRaw.Rewriter
{
    internal class RemoveReplaceRegexesFactory : IRemoveReplaceRegexesFactory
    {
        public IRemoveReplaceRegexes Create(RemoveReplaceSettings removeReplaceSettings)
        {
            RemoveCommentRegexes? removeCommentRegexes = null;
            if (removeReplaceSettings.RemoveCommentIdentifiers != null)
            {
                removeCommentRegexes = RemoveCommentRegexes.Create(removeReplaceSettings.RemoveCommentIdentifiers);
            }

            var regexRemovalOrReplacements = removeReplaceSettings.RemovalsOrReplacements.Select(r => RegexRemovalOrReplacement.Create(r)).ToList();
            return new RemoveReplaceRegexes(regexRemovalOrReplacements, removeCommentRegexes);
        }
    }
}
