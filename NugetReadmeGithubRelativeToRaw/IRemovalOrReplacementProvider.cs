using System.Collections.Generic;
using NugetReadmeGithubRelativeToRaw.Rewriter;

namespace NugetReadmeGithubRelativeToRaw
{
    internal interface IRemovalOrReplacementProvider
    {
        RemovalOrReplacement? Provide(MetadataItem metadataItem, List<string> errors);
    }
}
