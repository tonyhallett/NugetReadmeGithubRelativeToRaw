using NugetReadmeGithubRelativeToRaw;
using NugetReadmeGithubRelativeToRaw.Rewriter;

namespace UnitTests
{
    internal sealed class ConcatenatingArgumentsMessageProvider : IMessageProvider
    {
        public string UnsupportedImageDomain(string imageDomain) => imageDomain;

        public string CouldNotParseRepositoryUrl(string propertyValue) => propertyValue;

        public string CouldNotParseRewriteTagsOptionsUsingDefault(string propertyValue, RewriteTagsOptions defaultRewriteTagsOptions)
            => $"{propertyValue}{defaultRewriteTagsOptions}";

        public string ReadmeFileDoesNotExist(string readmeFilePath) => readmeFilePath;

        public string RemoveCommentsIdentifiersFormat()
        {
            throw new NotImplementedException();
        }

        public string RemoveCommentsIdentifiersSameStartEnd()
        {
            throw new NotImplementedException();
        }

        public string RequiredMetadata(string metadataName, string itemSpec) => $"{metadataName}{itemSpec}";

        public string UnsupportedCommentOrRegex(string itemSpec) => itemSpec;
    }
}
