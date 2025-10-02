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
            var removeReplaceSettingsResult = _removeReplaceSettingsProvider.Provide(null, null);

            Assert.Multiple(() =>
            {
                Assert.That(removeReplaceSettingsResult.Errors, Is.Empty);
                Assert.That(removeReplaceSettingsResult.Settings, Is.Null);
            });
        }

        [Test]
        public void Should_Provide_RemoveCommentIdentifiers_When_Specified()
        {
            string removeCommentIdentifiersMsBuild = "frommsbuild";
            _mockRemoveCommentsIdentifiers.Setup(parser => parser.Parse(removeCommentIdentifiersMsBuild, It.IsAny<IAddError>()))
                .Returns(new RemoveCommentIdentifiers("start", "end"));
            var removeReplaceSettingsResult = _removeReplaceSettingsProvider.Provide(null, removeCommentIdentifiersMsBuild);
            
            Assert.Multiple(() =>
            {
                Assert.That(removeReplaceSettingsResult.Errors, Is.Empty);
                var removeCommentIdentifiers = removeReplaceSettingsResult.Settings!.RemoveCommentIdentifiers;
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
            var metadataItem1 = SetUpItemWithMissingMetadata("missing1ItemSpec","missing1");
            var metadataItem2 = SetUpItemWithMissingMetadata("missing2ItemSpec", "missing2");

            var removeReplaceSettingsResult = _removeReplaceSettingsProvider.Provide([metadataItem1.TaskItem, metadataItem2.TaskItem], null);

            Assert.Multiple(() =>
            {
                Assert.That(removeReplaceSettingsResult.Errors, Has.Count.EqualTo(2));
                Assert.That(removeReplaceSettingsResult.Errors[0], Is.EqualTo("missing1missing1ItemSpec"));
                Assert.That(removeReplaceSettingsResult.Errors[1], Is.EqualTo("missing2missing2ItemSpec"));

                Assert.That(removeReplaceSettingsResult.Settings, Is.Null);

            });
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

        [Test]
        public void Should_Have_RemovalOrReplacement_From_Metadata()
        {
            var metadataItem1 = SetUpItemWithMissingMetadata("item1");
            var metadataItem2 = SetUpItemWithMissingMetadata("item2");

            var removalOrReplacement1 = new RemovalOrReplacement(CommentOrRegex.Comment, "start","end","replacement");
            var removalOrReplacement2 = new RemovalOrReplacement(CommentOrRegex.Regex, "start2", "end2", "replacemnet2");
            
            SetupRemovalOrReplacementProvider(removalOrReplacement1, metadataItem1);
            SetupRemovalOrReplacementProvider(removalOrReplacement2, metadataItem2);

            var removeReplaceSettingsResult = _removeReplaceSettingsProvider.Provide([metadataItem1.TaskItem, metadataItem2.TaskItem], null);

            Assert.Multiple(() =>
            {
                Assert.That(removeReplaceSettingsResult.Errors, Is.Empty);
                var removalsOrReplacements = removeReplaceSettingsResult.Settings!.RemovalsOrReplacements;
                Assert.That(removalsOrReplacements, Has.Count.EqualTo(2));
                Assert.That(removalsOrReplacements[0], Is.SameAs(removalOrReplacement1));
                Assert.That(removalsOrReplacements[1], Is.SameAs(removalOrReplacement2));
            });

            void SetupRemovalOrReplacementProvider(RemovalOrReplacement removalOrReplacement, MetadataItem matchingMetadataItem)
            {
                _mockRemovalOrReplacementProvider.Setup(removalOrReplacementProvider => removalOrReplacementProvider.Provide(It.Is<MetadataItem>(mi => mi.Metadata == matchingMetadataItem.Metadata && mi.TaskItem == matchingMetadataItem.TaskItem), It.IsAny<IAddError>()))
                    .Returns(removalOrReplacement);
            }
        }

        private MetadataItem SetUpItemWithMissingMetadata(string itemSpec, string? missingMetadataName = null)
        {
            var taskItem = new TaskItem(itemSpec);
            var metadata = new RemoveReplaceMetadata();
            if (missingMetadataName != null)
            {
                metadata.AddMissingMetadataName(missingMetadataName);
            }
            _mockMSBuildMetadataProvider.Setup(msBuildMetadataProvider => msBuildMetadataProvider.GetCustomMetadata<RemoveReplaceMetadata>(taskItem))
                .Returns(metadata);
            return new MetadataItem(metadata, taskItem);
        }

        [Test]
        public void Should_Get_RemovalOrReplacementProvider_Errors_But_Not_For_Items_With_Metadata_Name_Errors()
        {
            var metadataItem1 = SetUpItemWithMissingMetadata("missing1ItemSpec", "missing1");
            var metadataItem2 = SetUpItemWithMissingMetadata("okItemSpec");
            var setup = _mockRemovalOrReplacementProvider.Setup(removalOrReplacementProvider => removalOrReplacementProvider.Provide(It.Is<MetadataItem>(mi => mi.Metadata == metadataItem2.Metadata && mi.TaskItem == metadataItem2.TaskItem), It.IsAny<IAddError>()));
            setup.Verifiable();
            setup.Callback<MetadataItem, IAddError>((_, addError) => addError.AddError("error"));

            var removeReplaceSettingsResult = _removeReplaceSettingsProvider.Provide([metadataItem1.TaskItem, metadataItem2.TaskItem], null);

            _mockRemovalOrReplacementProvider.Verify();
            _mockRemovalOrReplacementProvider.VerifyNoOtherCalls();

            Assert.Multiple(() =>
            {
                Assert.That(removeReplaceSettingsResult.Errors, Has.Count.EqualTo(2));
                Assert.That(removeReplaceSettingsResult.Errors[0], Is.EqualTo("missing1missing1ItemSpec"));
                Assert.That(removeReplaceSettingsResult.Errors[1], Is.EqualTo("error"));
                Assert.That(removeReplaceSettingsResult.Settings, Is.Null);
            });
            
        }
    }
}
