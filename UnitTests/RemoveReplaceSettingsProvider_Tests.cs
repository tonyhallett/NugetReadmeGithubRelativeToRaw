using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
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
        private readonly ConcatenatingArgumentsMessageProvider _concatenatingArgumentsMessageProvider = new ConcatenatingArgumentsMessageProvider();
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
                _mockRemovalOrReplacementProvider.Object,
                _concatenatingArgumentsMessageProvider
                );
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
            _mockRemoveCommentsIdentifiers.Setup(parser => parser.Parse(removeCommentIdentifiersMsBuild, It.IsAny<IAddError>()))
                .Returns(new RemoveCommentIdentifiers("start", "end"));
            var removeCommentIdentifiers = _removeReplaceSettingsProvider.Provide(null, removeCommentIdentifiersMsBuild).Settings!.RemoveCommentIdentifiers;
            
            Assert.Multiple(() =>
            {
                Assert.That(removeCommentIdentifiers!.Start, Is.EqualTo("start"));
                Assert.That(removeCommentIdentifiers!.End, Is.EqualTo("end"));
            });
        }

        [Test]
        public void Should_Have_RemoveCommentsIdentifiersParser_Errors_And_Null_Settings()
        {
            string errorCausingremoveCommentIdentifiersMsBuild = "error";
            _mockRemoveCommentsIdentifiers.Setup(parser => parser.Parse(errorCausingremoveCommentIdentifiersMsBuild, It.IsAny<IAddError>()))
                .Returns((RemoveCommentIdentifiers?)null).Callback<string, IAddError>((_, addError) => addError.AddError("someerror"));
            var removeReplaceSettingsResult = _removeReplaceSettingsProvider.Provide(null, errorCausingremoveCommentIdentifiersMsBuild);

            Assert.Multiple(() =>
            {
                Assert.That(removeReplaceSettingsResult.Errors.Single(), Is.EqualTo("someerror"));
                Assert.That(removeReplaceSettingsResult.Settings, Is.Null);
            });
        }

        [Test]
        public void Should_Have_Missing_Metadata_Name_Errors_For_All_Items_And_Null_Settings()
        {
            var missingMetadataTaskItem1 = SetUpItemWithMissingMetadata("missing1ItemSpec","missing1");
            var missingMetadataTaskItem2 = SetUpItemWithMissingMetadata("missing2ItemSpec", "missing2");

            var removeReplaceSettingsResult = _removeReplaceSettingsProvider.Provide([missingMetadataTaskItem1, missingMetadataTaskItem2], null);

            Assert.Multiple(() =>
            {
                Assert.That(removeReplaceSettingsResult.Errors, Has.Count.EqualTo(2));
                Assert.That(removeReplaceSettingsResult.Errors[0], Is.EqualTo("missing1missing1ItemSpec"));
                Assert.That(removeReplaceSettingsResult.Errors[1], Is.EqualTo("missing2missing2ItemSpec"));

                Assert.That(removeReplaceSettingsResult.Settings, Is.Null);

            });

            ITaskItem SetUpItemWithMissingMetadata(string itemSpec, string missingMetadataName)
            {
                var missingMetadataTaskItem = new TaskItem(itemSpec);
                var missingMetadata = new RemoveReplaceMetadata();
                missingMetadata.AddMissingMetadataName(missingMetadataName);
                _mockMSBuildMetadataProvider.Setup(msBuildMetadataProvider => msBuildMetadataProvider.GetCustomMetadata<RemoveReplaceMetadata>(missingMetadataTaskItem))
                    .Returns(missingMetadata);
                return missingMetadataTaskItem;
            }
        }

        [Test]
        public void Should_Have_RemovalOrReplacementProvider_Errors_And_Null_Settings()
        {
            var noMissingMetadataNamesTaskItem = new TaskItem("ok");
            _mockRemovalOrReplacementProvider.Setup(removalOrReplacementProvider => removalOrReplacementProvider.Provide(It.IsAny<MetadataItem>(), It.IsAny<IAddError>()))
                .Callback<MetadataItem, IAddError>((_, addError) => addError.AddError("error"));
            _mockMSBuildMetadataProvider.Setup(msBuildMetadataProvider => msBuildMetadataProvider.GetCustomMetadata<RemoveReplaceMetadata>(noMissingMetadataNamesTaskItem))
                .Returns(new RemoveReplaceMetadata());

            var removeReplaceSettingsResult = _removeReplaceSettingsProvider.Provide([noMissingMetadataNamesTaskItem], null);

            Assert.Multiple(() =>
            {
                Assert.That(removeReplaceSettingsResult.Errors.Single(), Is.EqualTo("error"));
                Assert.That(removeReplaceSettingsResult.Settings, Is.Null);
            });
        }

        // Should_Get_Should_Have_RemovalOrReplacementProvider_Errors_But_Not_For_Items_With_Metadata_Name_Errors()
    }
}
