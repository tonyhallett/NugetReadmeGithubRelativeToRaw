using System;

namespace NugetReadmeGithubRelativeToRaw.MSBuildHelpers
{
    [AttributeUsage(AttributeTargets.Property)]
    internal class RequiredMetadataAttribute : Attribute
    {
    }
}
