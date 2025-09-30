using System.Collections;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace UnitTests.MSBuildTestHelpers
{
    /// <summary>
    /// Wraps TaskItem to allow setting ItemSpec modifiers
    /// </summary>
    internal class TestTaskItem : ITaskItem2
    {
        private readonly ItemSpecModifiersMetadata? itemSpecModifiers;
        private readonly TaskItem _taskItem;
        private ITaskItem2 TaskItem2 => _taskItem;

        public TestTaskItem(Dictionary<string,string>? metadata = null, string itemSpec = "itemspec", ItemSpecModifiersMetadata? itemSpecModifiers = null)
        {
            metadata = metadata ?? [];
            this.itemSpecModifiers = itemSpecModifiers;
            _taskItem = new TaskItem(itemSpec, metadata);
        }

        public string ItemSpec {
            get => _taskItem.ItemSpec;
            set => _taskItem.ItemSpec = value;
        }

        public ICollection MetadataNames => _taskItem.MetadataNames;

        public int MetadataCount => _taskItem.MetadataCount;

        public string EvaluatedIncludeEscaped {
            get => TaskItem2.EvaluatedIncludeEscaped;
            set => TaskItem2.EvaluatedIncludeEscaped = value;
        }

        public IDictionary CloneCustomMetadata() => _taskItem.CloneCustomMetadata();


        public IDictionary CloneCustomMetadataEscaped() => TaskItem2.CloneCustomMetadataEscaped();


        public void CopyMetadataTo(ITaskItem destinationItem) => _taskItem.CopyMetadataTo(destinationItem);

        // TaskItem
        /*
            public string GetMetadata(string metadataName)
            {
                string metadataValue = (this as ITaskItem2).GetMetadataValueEscaped(metadataName);
                return EscapingUtilities.UnescapeAll(metadataValue);
            }
        */
        public string GetMetadata(string metadataName)
        {
            var fromModifiers = TryGetFromItemSpecModifiers(metadataName);
            if (fromModifiers != null)
            {
                return fromModifiers;
            }
            return _taskItem.GetMetadata(metadataName);
        }


        public string GetMetadataValueEscaped(string metadataName)
        {
            var fromModifiers = TryGetFromItemSpecModifiers(metadataName);
            if (fromModifiers != null)
            {
                return fromModifiers;
            }
            return TaskItem2.GetMetadataValueEscaped(metadataName);
        }

        private string? TryGetFromItemSpecModifiers(string metadataName)
        {
            if(itemSpecModifiers == null)
            {
                return null;
            }

            if (string.Equals(metadataName, nameof(ItemSpecModifiersMetadata.FullPath), StringComparison.OrdinalIgnoreCase))
            {
                return itemSpecModifiers.FullPath;
            }

            if (string.Equals(metadataName, nameof(ItemSpecModifiersMetadata.RootDir), StringComparison.OrdinalIgnoreCase))
            {
                return itemSpecModifiers.RootDir;
            }

            if (string.Equals(metadataName, nameof(ItemSpecModifiersMetadata.FileName), StringComparison.OrdinalIgnoreCase))
            {
                return itemSpecModifiers.FileName;
            }

            if (string.Equals(metadataName, nameof(ItemSpecModifiersMetadata.Extension), StringComparison.OrdinalIgnoreCase))
            {
                return itemSpecModifiers.Extension;
            }

            if (string.Equals(metadataName, nameof(ItemSpecModifiersMetadata.Directory), StringComparison.OrdinalIgnoreCase))
            {
                return itemSpecModifiers.Directory;
            }

            if (string.Equals(metadataName, nameof(ItemSpecModifiersMetadata.RelativeDir), StringComparison.OrdinalIgnoreCase))
            {
                return itemSpecModifiers.RelativeDir;
            }

            if (string.Equals(metadataName, nameof(ItemSpecModifiersMetadata.ModifiedTime), StringComparison.OrdinalIgnoreCase))
            {
                return itemSpecModifiers.TryGetModifiedTime();
            }

            if (string.Equals(metadataName, nameof(ItemSpecModifiersMetadata.CreatedTime), StringComparison.OrdinalIgnoreCase))
            {
                return itemSpecModifiers.TryGetCreatedTime();
            }

            if (string.Equals(metadataName, nameof(ItemSpecModifiersMetadata.AccessedTime), StringComparison.OrdinalIgnoreCase))
            {
                return itemSpecModifiers.TryGetAccessedTime();
            }

            if (string.Equals(metadataName, nameof(ItemSpecModifiersMetadata.DefiningProjectFullPath), StringComparison.OrdinalIgnoreCase))
            {
                return itemSpecModifiers.DefiningProjectFullPath;
            }

            if (string.Equals(metadataName, nameof(ItemSpecModifiersMetadata.DefiningProjectDirectory), StringComparison.OrdinalIgnoreCase))
            {
                return itemSpecModifiers.DefiningProjectDirectory;
            }

            if (string.Equals(metadataName, nameof(ItemSpecModifiersMetadata.DefiningProjectName), StringComparison.OrdinalIgnoreCase))
            {
                return itemSpecModifiers.DefiningProjectName;
            }

            if (string.Equals(metadataName, nameof(ItemSpecModifiersMetadata.DefiningProjectExtension), StringComparison.OrdinalIgnoreCase))
            {
                return itemSpecModifiers.DefiningProjectExtension;
            }

            return null;
        }

        public void RemoveMetadata(string metadataName) => _taskItem.RemoveMetadata(metadataName);


        public void SetMetadata(string metadataName, string metadataValue) => _taskItem.SetMetadata(metadataName, metadataValue);


        public void SetMetadataValueLiteral(string metadataName, string metadataValue) => TaskItem2.SetMetadataValueLiteral(metadataName, metadataValue);   

    }
}
