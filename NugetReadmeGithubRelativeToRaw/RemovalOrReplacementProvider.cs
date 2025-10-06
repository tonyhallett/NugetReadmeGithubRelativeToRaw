using System;
using System.Collections.Generic;
using Microsoft.Build.Framework;
using NugetReadmeGithubRelativeToRaw.MSBuildHelpers;
using NugetReadmeGithubRelativeToRaw.Rewriter;

namespace NugetReadmeGithubRelativeToRaw
{
    internal class RemovalOrReplacementProvider : IRemovalOrReplacementProvider
    {
        private readonly IIOHelper _ioHelper;
        private readonly IMessageProvider _messageProvider;

        public RemovalOrReplacementProvider(IIOHelper ioHelper, IMessageProvider messageProvider)
        {
            _ioHelper = ioHelper;
            _messageProvider = messageProvider;
        }

        private sealed class StartEnd
        {
            public StartEnd(string start, string? end)
            {
                Start = start;
                End = end;
            }

            public string Start { get; }
            public string? End { get; }
        }

        public RemovalOrReplacement? Provide(MetadataItem metadataItem, IAddError addError)
        {
            if (TryParseCommentOrRegex(metadataItem, addError) is CommentOrRegex commentOrRegex &&
                    GetStartEnd(metadataItem, addError) is StartEnd startEnd)
            {
                var replacementText = GetReplacementTextFromItem(metadataItem);
                return new RemovalOrReplacement(
                        commentOrRegex,
                        startEnd.Start,
                        startEnd.End,
                        replacementText);
            }

            return null;
        }

        private CommentOrRegex? TryParseCommentOrRegex(MetadataItem metadataItem, IAddError addError)
        {
            if (!Enum.TryParse<CommentOrRegex>(metadataItem.Metadata.CommentOrRegex, out var commentOrRegex))
            {
                string error = _messageProvider.UnsupportedCommentOrRegex(metadataItem.TaskItem.ItemSpec);
                addError.AddError( error );
                return null;
            }
            return commentOrRegex;
        }

        private StartEnd? GetStartEnd(MetadataItem metadataItem, IAddError addError)
        {
            var start = metadataItem.Metadata.Start!;
            var endRaw = metadataItem.Metadata.End;
            var end = string.IsNullOrEmpty(endRaw) ? null : endRaw;
            if (start == end)
            {
                addError.AddError(_messageProvider.SameStartEndMetadata(metadataItem.TaskItem.ItemSpec));
                return null;
            }

            return new StartEnd(start, end);
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
    }
}
