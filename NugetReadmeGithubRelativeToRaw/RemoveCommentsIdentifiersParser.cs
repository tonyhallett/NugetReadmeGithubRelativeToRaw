using System.Collections.Generic;
using NugetReadmeGithubRelativeToRaw.Rewriter;

namespace NugetReadmeGithubRelativeToRaw
{
    internal class RemoveCommentsIdentifiersParser : IRemoveCommentsIdentifiersParser
    {
        internal const string NumPartsError = "{0} must have two semicolon delimited values: start and end.";
        internal const string EmptyPartsError = "{0} must have non-empty start and end values.";
        internal const string SamePartsError = "{0} must have different start to end";

        public RemoveCommentIdentifiers? Parse(string? removeCommentIdentifiers, List<string> errors)
        {
            if (string.IsNullOrWhiteSpace(removeCommentIdentifiers))
            {
                return null;
            }

            var parts = removeCommentIdentifiers!.Split(';');
            if (parts.Length != 2)
            {
                AddErrorFormat(NumPartsError);
                return null;
            }

            var start = parts[0].Trim();
            var end = parts[1].Trim();
            if (string.IsNullOrEmpty(start) || string.IsNullOrEmpty(end))
            {
                AddErrorFormat(EmptyPartsError);
                return null;
            }

            if (start == end)
            {
                AddErrorFormat(SamePartsError);
                return null;
            }

            return new RemoveCommentIdentifiers(start, end);

            void AddErrorFormat(string format)
            {
                errors.Add(string.Format(format, nameof(ReadmeRewriterTask.RemoveCommentIdentifiers)));
            }
        }
    }
}
