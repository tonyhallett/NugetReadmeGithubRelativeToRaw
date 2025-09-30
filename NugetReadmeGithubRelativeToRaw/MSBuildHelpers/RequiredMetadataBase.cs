using System.Collections.Generic;

namespace NugetReadmeGithubRelativeToRaw.MSBuildHelpers
{
    internal abstract class RequiredMetadataBase : IRequiredMetadata
    {
        private readonly List<string> missingMetadataNames = new List<string>();

        [IgnoreMetadata]
        public IReadOnlyList<string> MissingMetadataNames => missingMetadataNames;

        public void AddMissingMetadataName(string metadataName)
        {
            missingMetadataNames.Add(metadataName);
        }
    }
}
