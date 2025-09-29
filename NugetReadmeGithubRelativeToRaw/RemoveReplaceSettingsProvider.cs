using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Build.Framework;
using NugetReadmeGithubRelativeToRaw.Rewriter;

namespace NugetReadmeGithubRelativeToRaw
{
    internal class RemoveReplaceSettingsProvider : IRemoveReplaceSettingsProvider
    {
        internal const string NumPartsError = "RemoveCommentIdentifiers must have two semicolon delimited values: start and end.";
        internal const string EmptyPartsError = "RemoveCommentIdentifiers must have non-empty start and end values.";
        internal const string SamePartsError = "RemoveCommentIdentifiers must have different start to end";
        
        private readonly IIOHelper _ioHelper;

        public RemoveReplaceSettingsProvider(IIOHelper ioHelper) => _ioHelper = ioHelper;

        public IRemoveReplaceSettingsResult Provide(ITaskItem[]? removeReplaceItems, string? removeCommentIdentifiers)
        {
            var errors = new List<string>();
            var parsedRemoveCommentIdentifiers = ParseRemoveCommentIdentifiers(removeCommentIdentifiers, errors);
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

            var initialErrorsCount = errors.Count;
            foreach(var removeReplaceItem in removeReplaceItems)
            {
                var replacementText = GetReplacementTextFromItem(removeReplaceItem);
                var commentOrRegex = GetCommentOrRegexFromMetadata(removeReplaceItem, errors);
                if (!commentOrRegex.HasValue)
                {
                    break;
                }

                var startEnd = GetStartEndMetadata(removeReplaceItem, errors);
                if(startEnd == null)
                {
                    break;
                }

                removalReplacements.Add(
                    new RemovalOrReplacement(
                        commentOrRegex.Value,
                        startEnd.Value.start,
                        startEnd.Value.end,
                        replacementText));
            }

            if (errors.Count > initialErrorsCount)
            {
                removalReplacements.Clear();
            }
            
            return removalReplacements;
        }

        private string? GetReplacementTextFromMetadata(ITaskItem removeReplaceItem)
            => removeReplaceItem.GetMetadata(RemoveReplaceSettingsMetadataNames.ReplacementText);

        private string? GetReplacementTextFromItem(ITaskItem removeReplaceItem)
        {
            var replacementText = GetReplacementTextFromMetadata(removeReplaceItem);
            if (replacementText == null)
            {
                if (_ioHelper.FileExists(removeReplaceItem.ItemSpec))
                {
                    replacementText = _ioHelper.ReadAllText(removeReplaceItem.ItemSpec);
                }
                // if is removal then does not need to exist
            }
            return replacementText;
        }

        private string? GetCommentOrRegexFromMetadata(ITaskItem removeReplaceItem)
            => removeReplaceItem.GetMetadata(RemoveReplaceSettingsMetadataNames.CommentOrRegex);

        private CommentOrRegex? GetCommentOrRegexFromMetadata(ITaskItem removeReplaceItem, List<string> errors)
        {
            var commentOrRegexStr = GetCommentOrRegexFromMetadata(removeReplaceItem);
            if (commentOrRegexStr == null)
            {
                errors.Add($"CommentOrRegex metadata is required on RemoveReplace item '{removeReplaceItem.ItemSpec}'.");
                return null;
            }

            if (!Enum.TryParse<CommentOrRegex>(commentOrRegexStr, out var commentOrRegex))
            {
                errors.Add($"CommentOrRegex metadata on RemoveReplace item '{removeReplaceItem.ItemSpec}' should be {nameof(CommentOrRegex.Comment)} or {nameof(CommentOrRegex.Regex)}.");
                return null;
            }
            return commentOrRegex;
        }

        private string? GetStartFromMetadata(ITaskItem removeReplaceItem) => removeReplaceItem.GetMetadata(RemoveReplaceSettingsMetadataNames.Start);
        private (string start,string? end)? GetStartEndMetadata(ITaskItem removeReplaceItem, List<string> errors)
        {
            var start = GetStartFromMetadata(removeReplaceItem);
            if (start == null)
            {
                errors.Add($"Start metadata is required on RemoveReplace item '{removeReplaceItem.ItemSpec}'.");
                return null;
            }
            var end = removeReplaceItem.GetMetadata(RemoveReplaceSettingsMetadataNames.End);
            if (start == end)
            {
                errors.Add($"If End metadata is specified on RemoveReplace item '{removeReplaceItem.ItemSpec}' it must be different to Start.");
                return null;
            }

            return (start, end);
        }

        private RemoveCommentIdentifiers? ParseRemoveCommentIdentifiers(string? removeCommentIdentifiers, List<string> errors)
        {
            if (string.IsNullOrWhiteSpace(removeCommentIdentifiers))
            {
                return null;
            }
         
            var parts = removeCommentIdentifiers!.Split(';');
            if (parts.Length != 2)
            {
                errors.Add(NumPartsError);
                return null;
            }
            
            var start = parts[0].Trim();
            var end = parts[1].Trim();
            if (string.IsNullOrEmpty(start) || string.IsNullOrEmpty(end))
            {
                errors.Add(EmptyPartsError);
                return null;
            }
            
            if(start == end)
            {
                errors.Add(SamePartsError);
                return null;
            }
            
            return new RemoveCommentIdentifiers(start, end);
        }
    }
}
