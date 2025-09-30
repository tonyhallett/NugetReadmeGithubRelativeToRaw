using System;
using System.Collections.Generic;
using Microsoft.Build.Framework;
using NugetReadmeGithubRelativeToRaw.MSBuildHelpers;
using NugetReadmeGithubRelativeToRaw.Rewriter;

namespace NugetReadmeGithubRelativeToRaw
{
    internal class RemoveReplaceSettingsProvider : IRemoveReplaceSettingsProvider
    {
        internal const string RequiredMetadataErrorFormat = "{0} metadata is required on RemoveReplace item '{1}'.";
        internal const string UnsupportedCommentOrRegexMetadataErrorFormat = "CommentOrRegex metadata on RemoveReplace item '{0}' should be {1} or {2}.";

        private readonly IIOHelper _ioHelper;
        private readonly IMSBuildMetadataProvider _msBuildMetadataProvider;
        private readonly IRemoveCommentsIdentifiersParser _removeCommentsIdentifiersParser;

        private sealed class StartEnd {
            public StartEnd(string start, string? end)
            {
                Start = start;
                End = end;
            }

            public string Start { get; }
            public string? End { get; }
        }

        private sealed class MetadataItem
        {
            public MetadataItem(RemoveReplaceMetadata removeReplaceMetadata, ITaskItem taskItem)
            {
                Metadata = removeReplaceMetadata;
                TaskItem = taskItem;
            }

            public RemoveReplaceMetadata Metadata { get; }
            public ITaskItem TaskItem { get; }
        }

        public RemoveReplaceSettingsProvider(
            IIOHelper ioHelper, 
            IMSBuildMetadataProvider msBuildMetadataProvider,
            IRemoveCommentsIdentifiersParser removeCommentsIdentifiersParser
            )
        {
            _ioHelper = ioHelper;
            _msBuildMetadataProvider = msBuildMetadataProvider;
            _removeCommentsIdentifiersParser = removeCommentsIdentifiersParser;
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
                return new RemoveReplaceSettingsResult(null, errors.Count == 0 ? null : errors);
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
                if (TryParseCommentOrRegex(metadataItem, errors) is CommentOrRegex commentOrRegex && 
                    GetStartEnd(metadataItem, errors) is StartEnd startEnd)
                {
                    var replacementText = GetReplacementTextFromItem(metadataItem);
                    removalReplacements.Add(
                        new RemovalOrReplacement(
                            commentOrRegex,
                            startEnd.Start,
                            startEnd.End,
                            replacementText));
                }
                else
                {
                    break;
                }
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
                        AddFormatError(errors, RequiredMetadataErrorFormat, missingMetadataName, removeReplaceItem.ItemSpec);
                    }
                    return null;
                }
                metadataItems.Add(new MetadataItem(metadata, removeReplaceItem));
            }

            return metadataItems;
        }


        private string? GetReplacementTextFromItem(MetadataItem metadataItem)
        {
            var replacementText = metadataItem.Metadata.ReplacementText;
            if (string.IsNullOrEmpty(replacementText))
            {
                replacementText = TryReadReplacementFile(metadataItem.TaskItem);
            }
            return replacementText;
        }

        // if is removal then does not need to exist
        private string? TryReadReplacementFile(ITaskItem removeReplaceItem)
        {
            string? replacementText = null;
            var fullPath = removeReplaceItem.GetFullPath();
            if (_ioHelper.FileExists(fullPath))
            {
                replacementText = _ioHelper.ReadAllText(fullPath);
            }
            return replacementText;
        }

        private CommentOrRegex? TryParseCommentOrRegex(MetadataItem metadataItem, List<string> errors)
        {
            if (!Enum.TryParse<CommentOrRegex>(metadataItem.Metadata.CommentOrRegex, out var commentOrRegex))
            {
                AddFormatError(errors, UnsupportedCommentOrRegexMetadataErrorFormat, metadataItem.TaskItem.ItemSpec, nameof(CommentOrRegex.Comment), nameof(CommentOrRegex.Regex));
                return null;
            }
            return commentOrRegex;
        }

        private StartEnd? GetStartEnd(MetadataItem metadataItem, List<string> errors)
        {
            var start = metadataItem.Metadata.Start!;
            var endRaw = metadataItem.Metadata.End;
            var end = endRaw == string.Empty ? null : endRaw;
            if (start == end)
            {
                errors.Add($"If End metadata is specified on RemoveReplace item '{metadataItem.TaskItem.ItemSpec}' it must be different to Start.");
                return null;
            }

            return new StartEnd(start, end);
        }



        private void AddFormatError(List<string> errors, string format, params string[] args)
            => errors.Add(string.Format(format, args));
    }
}
