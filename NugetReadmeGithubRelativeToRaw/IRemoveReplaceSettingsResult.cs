using System.Collections.Generic;
using NugetReadmeGithubRelativeToRaw.Rewriter;

namespace NugetReadmeGithubRelativeToRaw
{
    internal interface IRemoveReplaceSettingsResult
    {
        IReadOnlyList<string> Errors { get; }
        RemoveReplaceSettings? Settings { get; }
    }
}