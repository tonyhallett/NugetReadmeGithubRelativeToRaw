using System.Diagnostics.CodeAnalysis;
using NugetReadmeGithubRelativeToRaw.Rewriter;

namespace NugetReadmeGithubRelativeToRaw
{
    [ExcludeFromCodeCoverage]
    internal class MessageProvider : IMessageProvider
    {
        public static MessageProvider Instance { get; } = new MessageProvider();

        public string CouldNotParseRepositoryUrl(string propertyValue)
            => $"Could not parse the {MsBuildPropertyItemNames.RepositoryUrlProperty}: {propertyValue}";

        public string CouldNotParseRewriteTagsOptionsUsingDefault(string propertyValue, RewriteTagsOptions defaultRewriteTagsOptions) 
            => $"Could not parse the {MsBuildPropertyItemNames.RewriteTagsOptionsProperty}: {propertyValue}. Using the default: {defaultRewriteTagsOptions}";

        public string ReadmeFileDoesNotExist(string readmeFilePath) => $"Readme file does not exist at '{readmeFilePath}'";

        public string ReadmeHasUnsupportedHTML() => "Readme has unsupported HTML";

        public string RemoveCommentsIdentifiersFormat() 
            => $"MSBuild Property {nameof(ReadmeRewriterTask.RemoveCommentIdentifiers)} must have two semicolon delimited values: start and end.";

        public string RemoveCommentsIdentifiersSameStartEnd() 
            => $"MSBuild Property {nameof(ReadmeRewriterTask.RemoveCommentIdentifiers)} must have different start to end";

        public string RequiredMetadata(string metadataName, string itemSpec)
            => $"Metadata, {metadataName}, is required on item {MsBuildPropertyItemNames.RemoveReplaceItem} '{itemSpec}'.";

        public string UnsupportedCommentOrRegex(string itemSpec) 
            => $"Unsupported {nameof(RemoveReplaceMetadata.CommentOrRegex)} metadata on item {MsBuildPropertyItemNames.RemoveReplaceItem} '{itemSpec}'. Supported values {nameof(CommentOrRegex.Comment)} | {nameof(CommentOrRegex.Regex)}";

        public string UnsupportedImageDomain(string imageDomain) => $"Unsupported image domain found in README: {imageDomain}";
    }
}
