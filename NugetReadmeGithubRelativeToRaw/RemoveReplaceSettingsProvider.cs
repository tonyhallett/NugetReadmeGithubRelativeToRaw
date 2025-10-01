using System.Collections.Generic;
using System.Linq;
using Microsoft.Build.Framework;
using NugetReadmeGithubRelativeToRaw.MSBuildHelpers;
using NugetReadmeGithubRelativeToRaw.Rewriter;

namespace NugetReadmeGithubRelativeToRaw
{
    internal class RemoveReplaceSettingsProvider : IRemoveReplaceSettingsProvider
    {
        private readonly IMSBuildMetadataProvider _msBuildMetadataProvider;
        private readonly IRemoveCommentsIdentifiersParser _removeCommentsIdentifiersParser;
        private readonly IRemovalOrReplacementProvider removalOrReplacementProvider;
        private readonly IMessageProvider messageProvider;

        public RemoveReplaceSettingsProvider(
            IMSBuildMetadataProvider msBuildMetadataProvider,
            IRemoveCommentsIdentifiersParser removeCommentsIdentifiersParser,
            IRemovalOrReplacementProvider removalOrReplacementProvider,
            IMessageProvider messageProvider
            )
        {
            _msBuildMetadataProvider = msBuildMetadataProvider;
            _removeCommentsIdentifiersParser = removeCommentsIdentifiersParser;
            this.removalOrReplacementProvider = removalOrReplacementProvider;
            this.messageProvider = messageProvider;
        }

        public IRemoveReplaceSettingsResult Provide(ITaskItem[]? removeReplaceItems, string? removeCommentIdentifiers)
        {
            var removeReplaceSettingsResult = new RemoveReplaceSettingsResult();
            var parsedRemoveCommentIdentifiers = _removeCommentsIdentifiersParser.Parse(removeCommentIdentifiers, removeReplaceSettingsResult);
            var removalOrReplacements = CreateRemovalOrReplacements(removeReplaceItems, removeReplaceSettingsResult);
            
            // if errors are added to and checked then this should not be necessary.
            if (removeReplaceSettingsResult.Errors.Count > 0 || parsedRemoveCommentIdentifiers == null && removalOrReplacements.Count == 0)
            {
                return removeReplaceSettingsResult;
            }

            removeReplaceSettingsResult.Settings = new RemoveReplaceSettings(
                parsedRemoveCommentIdentifiers, removalOrReplacements);
            return removeReplaceSettingsResult;
        }

        private List<RemovalOrReplacement> CreateRemovalOrReplacements(
            ITaskItem[]? removeReplaceItems, 
            IAddError addError)
        {
            var removalReplacements = new List<RemovalOrReplacement>();
            
            List<MetadataItem> metadataItemsWithoutMissingMetadata = GetMetadataItemsWithoutMissingMetadata(removeReplaceItems, addError);

            foreach (var metadataItemWithoutMissingMetadata in metadataItemsWithoutMissingMetadata)
            {
                var removalOrReplacement = removalOrReplacementProvider.Provide(metadataItemWithoutMissingMetadata, addError);
                if (removalOrReplacement != null)
                {
                    removalReplacements.Add(removalOrReplacement);
                }
            }

            return removalReplacements.Count == removeReplaceItems?.Length ? removalReplacements : Enumerable.Empty<RemovalOrReplacement>().ToList();
        }

        private List<MetadataItem> GetMetadataItemsWithoutMissingMetadata(ITaskItem[]? removeReplaceItems, IAddError addError)
        {
            List<MetadataItem> metadataItems = new List<MetadataItem>();
            if (removeReplaceItems == null)
            {
                return metadataItems;
            }

            foreach (var removeReplaceItem in removeReplaceItems)
            {
                var metadata = _msBuildMetadataProvider.GetCustomMetadata<RemoveReplaceMetadata>(removeReplaceItem);
                if (metadata.MissingMetadataNames.Count > 0)
                {
                    foreach (var missingMetadataName in metadata.MissingMetadataNames)
                    {
                        addError.AddError(messageProvider.RequiredMetadata(
                            missingMetadataName,
                            removeReplaceItem.ItemSpec
                            ));
                    }
                }
                else
                {
                    metadataItems.Add(new MetadataItem(metadata, removeReplaceItem));
                }
            }


            return metadataItems;
        }
    }
}
