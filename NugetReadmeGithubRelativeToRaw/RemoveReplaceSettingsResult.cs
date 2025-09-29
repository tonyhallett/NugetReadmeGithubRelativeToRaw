using System.Collections.Generic;
using NugetReadmeGithubRelativeToRaw.Rewriter;

namespace NugetReadmeGithubRelativeToRaw
{
    internal class RemoveReplaceSettingsResult : IRemoveReplaceSettingsResult
    {
        public RemoveReplaceSettings? Settings { get; }

        public RemoveReplaceSettingsResult(RemoveReplaceSettings? settings, IReadOnlyList<string>? errors)
        {
            Settings = settings;
            Errors = errors;
        }

        public IReadOnlyList<string>? Errors { get; }
    }
}
