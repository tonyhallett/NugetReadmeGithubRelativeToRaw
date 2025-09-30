namespace NugetReadmeGithubRelativeToRaw.MSBuildHelpers
{
    internal class ItemSpecModifiers
    {
        internal const string FullPath = "FullPath";
        internal const string RootDir = "RootDir";
        internal const string Filename = "Filename";
        internal const string Extension = "Extension";
        internal const string RelativeDir = "RelativeDir";
        internal const string Directory = "Directory";
        internal const string RecursiveDir = "RecursiveDir";
        internal const string Identity = "Identity";
        internal const string ModifiedTime = "ModifiedTime";
        internal const string CreatedTime = "CreatedTime";
        internal const string AccessedTime = "AccessedTime";
        internal const string DefiningProjectFullPath = "DefiningProjectFullPath";
        internal const string DefiningProjectDirectory = "DefiningProjectDirectory";
        internal const string DefiningProjectName = "DefiningProjectName";
        internal const string DefiningProjectExtension = "DefiningProjectExtension";

        internal static string[] All { get; } = new string[]
        {
            FullPath, RootDir, Filename, Extension, RelativeDir, Directory, RecursiveDir, 
            Identity, 
            ModifiedTime, CreatedTime, AccessedTime, 
            DefiningProjectFullPath, DefiningProjectDirectory, DefiningProjectName, DefiningProjectExtension
        };
    }
}
