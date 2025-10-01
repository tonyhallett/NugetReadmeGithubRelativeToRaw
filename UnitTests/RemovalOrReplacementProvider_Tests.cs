using Microsoft.Build.Utilities;
using Moq;
using NugetReadmeGithubRelativeToRaw;
using NugetReadmeGithubRelativeToRaw.Rewriter;
using UnitTests.MSBuildTestHelpers;

namespace UnitTests
{
    internal class RemovalOrReplacementProvider_Tests
    {
        private Mock<IIOHelper> _mockIOHelper;
        private RemovalOrReplacementProvider _removalOrReplacementProvider;

        [SetUp]
        public void SetUp()
        {
            _mockIOHelper = new Mock<IIOHelper>();
            _removalOrReplacementProvider = new RemovalOrReplacementProvider(_mockIOHelper.Object);
        }

        [TestCase(CommentOrRegex.Comment)]
        [TestCase(CommentOrRegex.Regex)]
        public void Should_Parse_To_CommentOrRegex(CommentOrRegex commentOrRegex)
        {
            var removeReplaceMetadata = new RemoveReplaceMetadata
            {
                CommentOrRegex = commentOrRegex.ToString(),
                Start = "start",
                ReplacementText = "...."
            };
            var taskItem = new TaskItem();

            var errors = new List<string>();
            var removalOrReplacement = _removalOrReplacementProvider.Provide(new MetadataItem(removeReplaceMetadata, taskItem), errors);

            Assert.Multiple(() =>
            {
                Assert.That(removalOrReplacement!.CommentOrRegex, Is.EqualTo(commentOrRegex));
                Assert.That(errors, Is.Empty);
            });
        }

        [Test]
        public void Should_Have_Error_When_Unsupported_CommentOrRegex()
        {
            var removeReplaceMetadata = new RemoveReplaceMetadata
            {
                CommentOrRegex = "unsupported",
                Start = "start",
                ReplacementText = "..."
            };
            var taskItem = new TaskItem();

            var errors = new List<string>();
            var removalOrReplacement = _removalOrReplacementProvider.Provide(new MetadataItem(removeReplaceMetadata, taskItem), errors);

            var expectedError = string.Format(
                RemovalOrReplacementProvider.UnsupportedCommentOrRegexMetadataErrorFormat,
                nameof(RemoveReplaceMetadata.CommentOrRegex),
                MsBuildPropertyItemNames.RemoveReplaceItem,
                taskItem.ItemSpec,
                nameof(CommentOrRegex.Comment),
                nameof(CommentOrRegex.Regex));
            Assert.Multiple(() =>
            {
                Assert.That(removalOrReplacement, Is.Null);
                Assert.That(errors.Single(), Is.EqualTo(expectedError));
            });
        }

        [Test]
        public void Should_Use_Start_From_Metadata()
        {
            var removeReplaceMetadata = new RemoveReplaceMetadata
            {
                CommentOrRegex = nameof(CommentOrRegex.Regex),
                Start = "startregex",
                ReplacementText = "..."
            };
            var taskItem = new TaskItem();

            var errors = new List<string>();
            var removalOrReplacement = _removalOrReplacementProvider.Provide(new MetadataItem(removeReplaceMetadata, taskItem), errors);

            Assert.Multiple(() =>
            {
                Assert.That(removalOrReplacement!.Start, Is.EqualTo("startregex"));
                Assert.That(errors, Is.Empty);
            });
        }

        [TestCase("")]
        [TestCase(null)]
        public void Should_Have_Null_End_When_Null_Or_Empty(string? end)
        {
            var removeReplaceMetadata = new RemoveReplaceMetadata
            {
                CommentOrRegex = nameof(CommentOrRegex.Regex),
                Start = "startregex",
                End = end,
                ReplacementText = "..."
            };
            var taskItem = new TaskItem();

            var errors = new List<string>();
            var removalOrReplacement = _removalOrReplacementProvider.Provide(new MetadataItem(removeReplaceMetadata, taskItem), errors);

            Assert.Multiple(() =>
            {
                Assert.That(removalOrReplacement!.End, Is.Null);
                Assert.That(errors, Is.Empty);
            });
        }

        [Test]
        public void Should_Use_End_From_Metadata_When_Specified()
        {
            var removeReplaceMetadata = new RemoveReplaceMetadata
            {
                CommentOrRegex = nameof(CommentOrRegex.Regex),
                Start = "startregex",
                End = "endregex",
                ReplacementText = "..."
            };
            var taskItem = new TaskItem();

            var errors = new List<string>();
            var removalOrReplacement = _removalOrReplacementProvider.Provide(new MetadataItem(removeReplaceMetadata, taskItem), errors);

            Assert.Multiple(() =>
            {
                Assert.That(removalOrReplacement!.End, Is.EqualTo("endregex"));
                Assert.That(errors, Is.Empty);
            });
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
            
            var errors = new List<string>();
            var removalOrReplacement = _removalOrReplacementProvider.Provide(new MetadataItem(removeReplaceMetadata, taskItem), errors);

            Assert.Multiple(() =>
            {
                Assert.That(removalOrReplacement!.ReplacementText, Is.EqualTo("replacement"));
                Assert.That(errors, Is.Empty);
            });
        }


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


            _mockIOHelper.Setup(ioHelper => ioHelper.FileExists("fullpath")).Returns(true);
            _mockIOHelper.Setup(ioHelper => ioHelper.ReadAllText("fullpath")).Returns("filereplacement");

            var errors = new List<string>();
            var removalOrReplacement = _removalOrReplacementProvider.Provide(new MetadataItem(removeReplaceMetadata, testTaskItem), errors);
            Assert.Multiple(() =>
            {
                Assert.That(removalOrReplacement!.ReplacementText, Is.EqualTo("filereplacement"));
                Assert.That(errors, Is.Empty);
            });
        }
    }
}
