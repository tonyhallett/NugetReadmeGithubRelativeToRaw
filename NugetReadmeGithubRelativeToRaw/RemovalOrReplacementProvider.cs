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
        private readonly IErrorProvider errorProvider;

        public RemovalOrReplacementProvider(IIOHelper ioHelper, IErrorProvider errorProvider)
        {
            _ioHelper = ioHelper;
            this.errorProvider = errorProvider;
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

        public RemovalOrReplacement? Provide(MetadataItem metadataItem, List<string> errors)
        {
            if (TryParseCommentOrRegex(metadataItem, errors) is CommentOrRegex commentOrRegex &&
                    GetStartEnd(metadataItem, errors) is StartEnd startEnd)
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

        private CommentOrRegex? TryParseCommentOrRegex(MetadataItem metadataItem, List<string> errors)
        {
            if (!Enum.TryParse<CommentOrRegex>(metadataItem.Metadata.CommentOrRegex, out var commentOrRegex))
            {
                string error = errorProvider.ProvideUnsupportedCommentOrRegex(
                    nameof(RemoveReplaceMetadata.CommentOrRegex),
                    MsBuildPropertyItemNames.RemoveReplaceItem,
                    metadataItem.TaskItem.ItemSpec,
                    $"{nameof(CommentOrRegex.Comment)} | {nameof(CommentOrRegex.Regex)}"
                    );
                errors.Add( error );
                return null;
            }
            return commentOrRegex;
        }

        private StartEnd? GetStartEnd(MetadataItem metadataItem, List<string> errors)
        {
            var start = metadataItem.Metadata.Start!;
            var endRaw = metadataItem.Metadata.End;
            var end = string.IsNullOrEmpty(endRaw) ? null : endRaw;
            if (start == end)
            {
                errors.Add($"If End metadata is specified on RemoveReplace item '{metadataItem.TaskItem.ItemSpec}' it must be different to Start.");
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
