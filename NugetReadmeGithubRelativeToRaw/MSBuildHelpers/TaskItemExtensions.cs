using Microsoft.Build.Framework;

namespace NugetReadmeGithubRelativeToRaw.MSBuildHelpers
{
    internal static class TaskItemExtensions
    {
        public static string GetFullPath(this ITaskItem taskItem)
        {
            return taskItem.GetMetadata(ItemSpecModifiers.FullPath);
        }

        public static string GetRootDir(this ITaskItem taskItem)
        {
            return taskItem.GetMetadata(ItemSpecModifiers.RootDir);
        }
        public static string GetFilename(this ITaskItem taskItem)
        {
            return taskItem.GetMetadata(ItemSpecModifiers.Filename);
        }

        public static string GetExtension(this ITaskItem taskItem)
        {
            return taskItem.GetMetadata(ItemSpecModifiers.Extension);
        }

        public static string GetRelativeDir(this ITaskItem taskItem)
        {
            return taskItem.GetMetadata(ItemSpecModifiers.RelativeDir);
        }

        public static string GetDirectory(this ITaskItem taskItem)
        {
            return taskItem.GetMetadata(ItemSpecModifiers.Directory);
        }

        public static string GetIdentity(this ITaskItem taskItem)
        {
            return taskItem.GetMetadata(ItemSpecModifiers.Identity);
        }

        public static string GetModifiedTime(this ITaskItem taskItem)
        {
            return taskItem.GetMetadata(ItemSpecModifiers.ModifiedTime);
        }

        public static string GetCreatedTime(this ITaskItem taskItem)
        {
            return taskItem.GetMetadata(ItemSpecModifiers.CreatedTime);
        }

        public static string GetAccessedTime(this ITaskItem taskItem)
        {
            return taskItem.GetMetadata(ItemSpecModifiers.AccessedTime);
        }

        public static string GetDefiningProjectFullPath(this ITaskItem taskItem)
        {
            return taskItem.GetMetadata(ItemSpecModifiers.DefiningProjectFullPath);
        }

        public static string GetDefiningProjectDirectory(this ITaskItem taskItem)
        {
            return taskItem.GetMetadata(ItemSpecModifiers.DefiningProjectDirectory);
        }

        public static string GetDefiningProjectName(this ITaskItem taskItem)
        {
            return taskItem.GetMetadata(ItemSpecModifiers.DefiningProjectName);
        }

        public static string GetDefiningProjectExtension(this ITaskItem taskItem)
        {
            return taskItem.GetMetadata(ItemSpecModifiers.DefiningProjectExtension);
        }

        public static string GetFullPathEscaped(this ITaskItem2 taskItem)
        {
            return taskItem.GetMetadataValueEscaped(ItemSpecModifiers.FullPath);
        }

        public static string GetRootDirEscaped(this ITaskItem2 taskItem)
        {
            return taskItem.GetMetadataValueEscaped(ItemSpecModifiers.RootDir);
        }
        public static string GetFilenameEscaped(this ITaskItem2 taskItem)
        {
            return taskItem.GetMetadataValueEscaped(ItemSpecModifiers.Filename);
        }

        public static string GetExtension(this ITaskItem2 taskItem)
        {
            return taskItem.GetMetadata(ItemSpecModifiers.Extension);
        }

        public static string GetDirectoryEscaped(this ITaskItem2 taskItem)
        {
            return taskItem.GetMetadataValueEscaped(ItemSpecModifiers.Directory);
        }

        public static string GetIdentityEscaped(this ITaskItem2 taskItem)
        {
            return taskItem.GetMetadataValueEscaped(ItemSpecModifiers.Identity);
        }

        public static string GetDefiningProjectFullPathEscaped(this ITaskItem2 taskItem)
        {
            return taskItem.GetMetadataValueEscaped(ItemSpecModifiers.DefiningProjectFullPath);
        }

        public static string GetDefiningProjectDirectoryEscaped(this ITaskItem2 taskItem)
        {
            return taskItem.GetMetadataValueEscaped(ItemSpecModifiers.DefiningProjectDirectory);
        }
    }
}
