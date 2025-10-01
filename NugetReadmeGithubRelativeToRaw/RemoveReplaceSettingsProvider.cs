using System.Collections.Generic;
using Microsoft.Build.Framework;
using NugetReadmeGithubRelativeToRaw.MSBuildHelpers;
using NugetReadmeGithubRelativeToRaw.Rewriter;

namespace NugetReadmeGithubRelativeToRaw
{
    internal class RemoveReplaceSettingsProvider : IRemoveReplaceSettingsProvider
    {
        internal const string RequiredMetadataErrorFormat = "{0} metadata is required on {1} item '{2}'.";

        private readonly IMSBuildMetadataProvider _msBuildMetadataProvider;
        private readonly IRemoveCommentsIdentifiersParser _removeCommentsIdentifiersParser;
        private readonly IRemovalOrReplacementProvider removalOrReplacementProvider;

        public RemoveReplaceSettingsProvider(
            IMSBuildMetadataProvider msBuildMetadataProvider,
            IRemoveCommentsIdentifiersParser removeCommentsIdentifiersParser,
            IRemovalOrReplacementProvider removalOrReplacementProvider
            )
        {
            _msBuildMetadataProvider = msBuildMetadataProvider;
            _removeCommentsIdentifiersParser = removeCommentsIdentifiersParser;
            this.removalOrReplacementProvider = removalOrReplacementProvider;
        }

        public IRemoveReplaceSettingsResult Provide(ITaskItem[]? removeReplaceItems, string? removeCommentIdentifiers)
        {
            var errors = new List<string>();
            var parsedRemoveCommentIdentifiers = _removeCommentsIdentifiersParser.Parse(removeCommentIdentifiers, errors);
            if (errors.Count > 0)
            {
                return new RemoveReplaceSettingsResult(null, errors);
            }

            var removalOrReplacements = CreateRemovalOrReplacements(removeReplaceItems, errors);
            if (parsedRemoveCommentIdentifiers == null && removalOrReplacements.Count == 0)
            {
                return new RemoveReplaceSettingsResult(null, errors);
            }

            return new RemoveReplaceSettingsResult(new RemoveReplaceSettings(parsedRemoveCommentIdentifiers, removalOrReplacements), errors);
        }

        private List<RemovalOrReplacement> CreateRemovalOrReplacements(ITaskItem[]? removeReplaceItems, List<string> errors)
        {
            var removalReplacements = new List<RemovalOrReplacement>();
            if (removeReplaceItems == null)
            {
                return removalReplacements;
            }

            List<MetadataItem>? metadataItems = GetMetadataItems(removeReplaceItems, errors);
            if (metadataItems == null)
            {
                return removalReplacements;
            }
            
            var initialErrorsCount = errors.Count;
            foreach (var metadataItem in metadataItems)
            {
                var removalOrReplacement = removalOrReplacementProvider.Provide(metadataItem, errors);
                if(removalOrReplacement == null)
                {
                    break;
                }
                removalReplacements.Add(removalOrReplacement);
            }

            if (errors.Count > initialErrorsCount)
            {
                removalReplacements.Clear();
            }
            
            return removalReplacements;
        }


        private List<MetadataItem>? GetMetadataItems(ITaskItem[] removeReplaceItems, List<string> errors)
        {
            List<MetadataItem> metadataItems = new List<MetadataItem>();
            foreach (var removeReplaceItem in removeReplaceItems)
            {
                var metadata = _msBuildMetadataProvider.GetCustomMetadata<RemoveReplaceMetadata>(removeReplaceItem);
                if (metadata.MissingMetadataNames.Count > 0)
                {
                    foreach (var missingMetadataName in metadata.MissingMetadataNames)
                    {
                        errors.Add(
                            string.Format(
                                RequiredMetadataErrorFormat,
                                MsBuildPropertyItemNames.RemoveReplaceItem, 
                                missingMetadataName, 
                                removeReplaceItem.ItemSpec));
                    }
                    return null;
                }
                metadataItems.Add(new MetadataItem(metadata, removeReplaceItem));
            }

            return metadataItems;
        }
    }
}
