using Microsoft.Build.Framework;

namespace NugetReadmeGithubRelativeToRaw
{
    internal interface IRemoveReplaceSettingsProvider
    {
        IRemoveReplaceSettingsResult Provide(ITaskItem[]? removeReplaceItems, string? removeCommentIdentifiers);
    }
}
