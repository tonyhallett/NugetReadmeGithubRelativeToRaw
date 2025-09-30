using Microsoft.Build.Framework;

namespace NugetReadmeGithubRelativeToRaw.MSBuildHelpers
{
    internal interface IMSBuildMetadataProvider
    {
        T GetCustomMetadata<T>(ITaskItem item) where T : new();
    }
}
