using System.Collections.Generic;
using NugetReadmeGithubRelativeToRaw.Rewriter;

namespace NugetReadmeGithubRelativeToRaw
{
    internal class RemoveReplaceSettingsResult : IRemoveReplaceSettingsResult, IAddError
    {
        private readonly List<string> _errors = new List<string>();

        public RemoveReplaceSettings? Settings { get; set; }
        
        public IReadOnlyList<string> Errors => _errors;

        public void AddError(string message)
        {
            _errors.Add(message);
        }
    }
}
