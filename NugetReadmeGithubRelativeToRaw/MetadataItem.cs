using Microsoft.Build.Framework;

namespace NugetReadmeGithubRelativeToRaw
{
    internal sealed class MetadataItem
    {
        public MetadataItem(RemoveReplaceMetadata removeReplaceMetadata, ITaskItem taskItem)
        {
            Metadata = removeReplaceMetadata;
            TaskItem = taskItem;
        }

        public RemoveReplaceMetadata Metadata { get; }
        public ITaskItem TaskItem { get; }
    }
}
