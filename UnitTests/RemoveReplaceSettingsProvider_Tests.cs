using Microsoft.Build.Framework;
using Moq;
using NugetReadmeGithubRelativeToRaw;
using NugetReadmeGithubRelativeToRaw.MSBuildHelpers;
using NugetReadmeGithubRelativeToRaw.Rewriter;

namespace UnitTests
{
    internal class RemoveReplaceSettingsProvider_Tests
    {
        private Mock<IRemovalOrReplacementProvider> _mockRemovalOrReplacementProvider;
        private Mock<IMSBuildMetadataProvider> _mockMSBuildMetadataProvider;
        private Mock<IRemoveCommentsIdentifiersParser> _mockRemoveCommentsIdentifiers;
        private RemoveReplaceSettingsProvider _removeReplaceSettingsProvider;

        [SetUp]
        public void SetUp()
        {
            _mockRemovalOrReplacementProvider = new Mock<IRemovalOrReplacementProvider>();
            _mockMSBuildMetadataProvider = new Mock<IMSBuildMetadataProvider>();
            _mockRemoveCommentsIdentifiers = new Mock<IRemoveCommentsIdentifiersParser>();
            _removeReplaceSettingsProvider = new RemoveReplaceSettingsProvider(
                _mockMSBuildMetadataProvider.Object, 
                _mockRemoveCommentsIdentifiers.Object,
                _mockRemovalOrReplacementProvider.Object);
        }

        [Test]
        public void Should_Provide_Null_When_Not_Specified()
        {
            var removeReplaceSettings = _removeReplaceSettingsProvider.Provide(null, null).Settings;

            Assert.That(removeReplaceSettings, Is.Null);
        }

        [Test]
        public void Should_Provide_RemoveCommentIdentifiers_When_Specified()
        {
            string removeCommentIdentifiersMsBuild = "frommsbuild";
            _mockRemoveCommentsIdentifiers.Setup(parser => parser.Parse(removeCommentIdentifiersMsBuild, It.IsAny<List<string>>()))
                .Returns(new RemoveCommentIdentifiers("start", "end"));
            var removeCommentIdentifiers = _removeReplaceSettingsProvider.Provide(null, removeCommentIdentifiersMsBuild).Settings!.RemoveCommentIdentifiers;
            
            Assert.Multiple(() =>
            {
                Assert.That(removeCommentIdentifiers!.Start, Is.EqualTo("start"));
                Assert.That(removeCommentIdentifiers!.End, Is.EqualTo("end"));
            });
        }

        /*
                private void SetupRemoveReplaceMetadata(RemoveReplaceMetadata removeReplaceMetadata, ITaskItem taskItem)
            => _mockMSBuildMetadataProvider.Setup(msBuildMetadataProvider => msBuildMetadataProvider.GetCustomMetadata<RemoveReplaceMetadata>(taskItem)).Returns(removeReplaceMetadata);

        */


    }
}
