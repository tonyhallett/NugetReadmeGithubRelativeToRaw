using NugetReadmeGithubRelativeToRaw;
using NugetReadmeGithubRelativeToRaw.Rewriter;

namespace UnitTests
{
    internal sealed class ConcatenatingArgumentsMessageProvider : IMessageProvider
    {
        public string UnsupportedImageDomain(string imageDomain) => imageDomain;

        public string CouldNotParseRepositoryUrl(string? propertyValue) => propertyValue ?? "null";

        public string CouldNotParseRewriteTagsOptionsUsingDefault(string propertyValue, RewriteTagsOptions defaultRewriteTagsOptions)
            => $"{propertyValue}{defaultRewriteTagsOptions}";

        public string ReadmeFileDoesNotExist(string readmeFilePath) => readmeFilePath;

        public string RemoveCommentsIdentifiersFormat() => nameof(RemoveCommentsIdentifiersFormat);

        public string RemoveCommentsIdentifiersSameStartEnd() => nameof(RemoveCommentsIdentifiersSameStartEnd);

        public string RequiredMetadata(string metadataName, string itemSpec) => $"{metadataName}{itemSpec}";

        public string UnsupportedCommentOrRegex(string itemSpec) => itemSpec;

        public string ReadmeHasUnsupportedHTML() => nameof(ReadmeHasUnsupportedHTML);

        public string SameStartEndMetadata(string itemSpec) => itemSpec;

        public string MissingReadmeAsset(string missingReadmeAsset) => missingReadmeAsset;
    }
}
