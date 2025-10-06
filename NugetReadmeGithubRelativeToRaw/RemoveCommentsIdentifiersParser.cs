using NugetReadmeGithubRelativeToRaw.Rewriter;

namespace NugetReadmeGithubRelativeToRaw
{
    internal class RemoveCommentsIdentifiersParser : IRemoveCommentsIdentifiersParser
    {
        private readonly IMessageProvider _messageProvider;

        public RemoveCommentsIdentifiersParser(IMessageProvider messageProvider) => _messageProvider = messageProvider;

        public RemoveCommentIdentifiers? Parse(string? removeCommentIdentifiers, IAddError addError)
        {
            if (string.IsNullOrEmpty(removeCommentIdentifiers))
            {
                return null;
            }

            var parts = removeCommentIdentifiers!.Split(';');
            if (parts.Length != 2)
            {
                addError.AddError(_messageProvider.RemoveCommentsIdentifiersFormat());
                return null;
            }

            var start = parts[0].Trim();
            var end = parts[1].Trim();
            if (string.IsNullOrEmpty(start) || string.IsNullOrEmpty(end))
            {
                addError.AddError(_messageProvider.RemoveCommentsIdentifiersFormat());
                return null;
            }

            if (start == end)
            {
                addError.AddError(_messageProvider.RemoveCommentsIdentifiersSameStartEnd());
                return null;
            }

            return new RemoveCommentIdentifiers(start, end);
        }
    }
}
