using NugetReadmeGithubRelativeToRaw.Rewriter;

namespace NugetReadmeGithubRelativeToRaw
{
    internal interface IMessageProvider
    {
        string RequiredMetadata(string metadataName, string itemSpec);

        string UnsupportedCommentOrRegex(string itemSpec);

        string RemoveCommentsIdentifiersFormat();

        string RemoveCommentsIdentifiersSameStartEnd();

        string UnsupportedImageDomain(string imageDomain);

        string ReadmeFileDoesNotExist(string readmeFilePath);

        string CouldNotParseRepositoryUrl(string propertyValue);

        string CouldNotParseRewriteTagsOptionsUsingDefault(string propertyValue, RewriteTagsOptions defaultRewriteTagsOptions);
    }
}
