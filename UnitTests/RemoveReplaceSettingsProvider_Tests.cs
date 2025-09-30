using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Moq;
using NugetReadmeGithubRelativeToRaw;
using NugetReadmeGithubRelativeToRaw.MSBuildHelpers;
using NugetReadmeGithubRelativeToRaw.Rewriter;
using UnitTests.MSBuildTestHelpers;

namespace UnitTests
{
    internal class RemoveReplaceSettingsProvider_Tests
    {
        private Mock<IIOHelper> _mockIOHelper;
        private Mock<IMSBuildMetadataProvider> _mockMSBuildMetadataProvider;
        private Mock<IRemoveCommentsIdentifiersParser> _mockRemoveCommentsIdentifiersMock;
        private RemoveReplaceSettingsProvider _removeReplaceSettingsProvider;

        [SetUp]
        public void SetUp()
        {
            _mockIOHelper = new Mock<IIOHelper>();
            _mockMSBuildMetadataProvider = new Mock<IMSBuildMetadataProvider>();
            _mockRemoveCommentsIdentifiersMock = new Mock<IRemoveCommentsIdentifiersParser>();
            _removeReplaceSettingsProvider = new RemoveReplaceSettingsProvider(_mockIOHelper.Object, _mockMSBuildMetadataProvider.Object, _mockRemoveCommentsIdentifiersMock.Object);
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
            _mockRemoveCommentsIdentifiersMock.Setup(parser => parser.Parse(removeCommentIdentifiersMsBuild, It.IsAny<List<string>>()))
                .Returns(new RemoveCommentIdentifiers("start", "end"));
            var removeCommentIdentifiers = _removeReplaceSettingsProvider.Provide(null, removeCommentIdentifiersMsBuild).Settings!.RemoveCommentIdentifiers;
            
            Assert.Multiple(() =>
            {
                Assert.That(removeCommentIdentifiers!.Start, Is.EqualTo("start"));
                Assert.That(removeCommentIdentifiers!.End, Is.EqualTo("end"));
            });
        }

        // task items => RemovalOrReplacement

        private RemovalOrReplacement Provide(ITaskItem taskItem)
        {
            ITaskItem[] taskItems = [taskItem];
            var result = _removeReplaceSettingsProvider.Provide(taskItems, null);

            return result.Settings!.RemovalsOrReplacements.Single();
        }

        [Test]
        public void Should_Use_ReplacementText_From_Metadata()
        {
            var removeReplaceMetadata = new RemoveReplaceMetadata
            {
                CommentOrRegex = nameof(CommentOrRegex.Comment),
                Start = "start",
                ReplacementText = "replacement"
            };
            var taskItem = new TaskItem();
            SetupRemoveReplaceMetadata(removeReplaceMetadata, taskItem);

            Assert.That(Provide(taskItem).ReplacementText, Is.EqualTo("replacement"));
        }

        private void SetupRemoveReplaceMetadata(RemoveReplaceMetadata removeReplaceMetadata, ITaskItem taskItem)
            => _mockMSBuildMetadataProvider.Setup(msBuildMetadataProvider => msBuildMetadataProvider.GetCustomMetadata<RemoveReplaceMetadata>(taskItem)).Returns(removeReplaceMetadata);

        [Test]
        public void Should_Use_ReplacementText_From_FileSystem_When_No_Metadata()
        {
            var removeReplaceMetadata = new RemoveReplaceMetadata
            {
                CommentOrRegex = nameof(CommentOrRegex.Comment),
                Start = "start",
                ReplacementText = ""
            };
            var testTaskItem = new TestTaskItem(null, "itemspec", new ItemSpecModifiersMetadata
            {
                FullPath = "fullpath"
            });
            SetupRemoveReplaceMetadata(removeReplaceMetadata, testTaskItem);


            _mockIOHelper.Setup(ioHelper => ioHelper.FileExists("fullpath")).Returns(true);
            _mockIOHelper.Setup(ioHelper => ioHelper.ReadAllText("fullpath")).Returns("filereplacement");

            Assert.That(Provide(testTaskItem).ReplacementText, Is.EqualTo("filereplacement"));
        }
    }
}
