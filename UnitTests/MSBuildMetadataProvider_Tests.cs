using Microsoft.Build.Utilities;
using NugetReadmeGithubRelativeToRaw;
using NugetReadmeGithubRelativeToRaw.MSBuildHelpers;
using NugetReadmeGithubRelativeToRaw.Rewriter;

namespace UnitTests
{
    internal class MSBuildMetadataProvider_Tests
    {
        [Test]
        public void Should_Reflect()
        {
            var msBuildMetadataProvider = new MSBuildMetadataProvider();

            var msBuildRemoveReplaceMetadata = new Dictionary<string, string>
            {
                // required
                { nameof(RemoveReplaceMetadata.CommentOrRegex), CommentOrRegex.Comment.ToString() },
                // required start missing
                // other missing are not required
            };
            var taskItem = new TaskItem("", msBuildRemoveReplaceMetadata);
            var removeReplaceMetadata = msBuildMetadataProvider.GetCustomMetadata<RemoveReplaceMetadata>(taskItem);

            Assert.Multiple(() =>
            {
                Assert.That(removeReplaceMetadata.CommentOrRegex, Is.EqualTo(CommentOrRegex.Comment.ToString()));
                Assert.That(removeReplaceMetadata.Start, Is.Empty);
                Assert.That(removeReplaceMetadata.End, Is.Empty);
                Assert.That(removeReplaceMetadata.ReplacementText, Is.Empty);
                Assert.That(removeReplaceMetadata.MissingMetadataNames.Single(), Is.EqualTo(nameof(RemoveReplaceMetadata.Start)));
            });
        }
    }
}
