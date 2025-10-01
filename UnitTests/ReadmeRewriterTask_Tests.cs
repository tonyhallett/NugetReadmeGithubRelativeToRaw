using Microsoft.Build.Framework;
using Moq;
using NugetReadmeGithubRelativeToRaw;
using NugetReadmeGithubRelativeToRaw.Rewriter;
using UnitTests.MSBuildTestHelpers;

namespace UnitTests
{
    internal class ReadmeRewriterTask_Tests
    {
        private const string repositoryUrl = "repositoryurl";
        private const string repositoryBranch = "repositorybranch";
        private const string projectDirectoryPath = "projectdir";
        private const string removeCommentIdentifiers = "removeCommentIdentifiers";
        private readonly ITaskItem[] removeReplaceTaskItems = [new Mock<ITaskItem>().Object];
        private DummyIOHelper _ioHelper;
        private Mock<IRemoveReplaceSettingsProvider> _mockRemoveReplaceSettingsProvider;
        private Mock<IReadmeRewriter> _mockReadmeRewriter;
        private DummyLogBuildEngine _dummyLogBuildEngine;
        private ReadmeRewriterTask _readmeRewriterTask;
        private TestRemoveReplaceSettingsResult removeReplaceSettingsResult;

        private class TestRemoveReplaceSettingsResult : IRemoveReplaceSettingsResult
        {
            public IReadOnlyList<string> Errors { get; set; } = [];
            public RemoveReplaceSettings? Settings { get; set; }
        }

        

        private sealed class DummyIOHelper : IIOHelper
        {
            public const string ReadmeText = "readme";
            public bool DoesFileExist { get; set; }

            public string CombinePaths(string path1, string path2)
            {
                return $"{path1};{path2}";
            }

            public string? FileExistsPath { get; private set; }
            
            public bool FileExists(string filePath)
            {
                FileExistsPath = filePath;
                return DoesFileExist;
            }

            public string ReadAllText(string readmePath)
            {
                return ReadmeText;
            }

            public string? WriteOutputReadme { get; private set; }
            public string? WriteRewrittenReadme { get; private set; }
            public void WriteAllText(string outputReadme, string rewrittenReadme)
            {
                WriteOutputReadme = outputReadme;
                WriteRewrittenReadme = rewrittenReadme;
            }
        }

        [SetUp]
        public void Setup()
        {
            _ioHelper = new DummyIOHelper();
            _mockRemoveReplaceSettingsProvider = new Mock<IRemoveReplaceSettingsProvider>();
            _mockReadmeRewriter = new Mock<IReadmeRewriter>();
            _dummyLogBuildEngine = new DummyLogBuildEngine();
            _readmeRewriterTask = new ReadmeRewriterTask
            {
                BuildEngine = _dummyLogBuildEngine,
                IOHelper = _ioHelper,
                ReadmeRewriter = _mockReadmeRewriter.Object,
                RemoveReplaceSettingsProvider = _mockRemoveReplaceSettingsProvider.Object,
                MessageProvider = new ConcatenatingArgumentsMessageProvider(),
                RepositoryUrl = repositoryUrl,
                RepositoryBranch = repositoryBranch,
                ProjectDirectoryPath = projectDirectoryPath
            };
            removeReplaceSettingsResult = new TestRemoveReplaceSettingsResult();
            _mockRemoveReplaceSettingsProvider.Setup(removeReplaceSettingsProvider => removeReplaceSettingsProvider.Provide(removeReplaceTaskItems, removeCommentIdentifiers))
                .Returns(removeReplaceSettingsResult);
            _readmeRewriterTask.RemoveReplaceItems = removeReplaceTaskItems;
            _readmeRewriterTask.RemoveCommentIdentifiers = removeCommentIdentifiers;
            
        }

        [Test]
        public void Should_Look_For_The_Readme_Relative_To_The_ProjectDirectoryPath()
        {
            _readmeRewriterTask.ReadmeRelativePath = "relativeReadme.md";

            _ = _readmeRewriterTask.Execute();

            Assert.That(_ioHelper.FileExistsPath, Is.EqualTo("projectdir;relativeReadme.md"));
        }

        [Test]
        public void Should_Default_The_ReadmeRelative_Path_To_Readme_In_Root()
        {
            _ = _readmeRewriterTask.Execute();

            Assert.That(_ioHelper.FileExistsPath, Is.EqualTo("projectdir;readme.md"));
        }

        [Test]
        public void Should_Log_Error_If_Readme_Does_Not_Exist()
        {
            var result = _readmeRewriterTask.Execute();

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(false));
                Assert.That(
                    _dummyLogBuildEngine.SingleErrorMessage(), 
                    Is.EqualTo("projectdir;readme.md"));
            });
        }

        private bool ExecuteReadmeExists()
        {
            _ioHelper.DoesFileExist = true;
            return _readmeRewriterTask.Execute();
        }

        [Test]
        public void Should_Rewrite_The_Readme_File_When_Exists()
        {
            _readmeRewriterTask.ReadmeRelativePath = "relativeReadme.md";
            var result = ExecuteReadmeExists();

            _mockReadmeRewriter.Verify(readmeRewriter => readmeRewriter.Rewrite(DummyIOHelper.ReadmeText, _readmeRewriterTask.ReadmeRelativePath, repositoryUrl, repositoryBranch, It.IsAny<RewriteTagsOptions>(), It.IsAny<RemoveReplaceSettings>()));
        }

        [Test]
        public void Should_Log_Error_When_RepositoryUrl_Cannot_Be_Parsed()
        {
            var result = ExecuteReadmeExists();

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(false));
                Assert.That(
                    _dummyLogBuildEngine.SingleErrorMessage(),
                    Is.EqualTo(repositoryUrl));
            });
        }

        [Test]
        public void Should_Log_Error_For_Every_Unsupported_Image_Domain()
        {
            ReadmeRewriterResult readmeRewriterResult = new ReadmeRewriterResult("rewrittenReadme", ["unsupported1", "unsupported2"]);
            _mockReadmeRewriter.Setup(readmeRewriter => readmeRewriter.Rewrite(
                It.IsAny<string>(),
                It.IsAny<string>(),
                repositoryUrl,
                repositoryBranch,
                It.IsAny<RewriteTagsOptions>(),
                It.IsAny<RemoveReplaceSettings>())).Returns(readmeRewriterResult);

            var result = ExecuteReadmeExists();

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(false));
                foreach (var (unsupportedImageDomain, error) in readmeRewriterResult.UnsupportedImageDomains.Zip(_dummyLogBuildEngine.ErrorMessages()))
                {
                    Assert.That(error, Is.EqualTo(unsupportedImageDomain));
                }
            });
        }

        [Test]
        public void Should_Write_Rewritten_Readme_To_OutputReadme()
        {
            _readmeRewriterTask.OutputReadme = "outputReadme.md";
            removeReplaceSettingsResult.Settings = new RemoveReplaceSettings(null, []);
            ReadmeRewriterResult readmeRewriterResult = new ReadmeRewriterResult("rewrittenReadme", []);
            _mockReadmeRewriter.Setup(readmeRewriter => readmeRewriter.Rewrite(
                It.IsAny<string>(),
                It.IsAny<string>(),
                repositoryUrl,
                repositoryBranch,
                It.IsAny<RewriteTagsOptions>(),
                removeReplaceSettingsResult.Settings)).Returns(readmeRewriterResult);

            var result = ExecuteReadmeExists();

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(true));
                Assert.That(_ioHelper.WriteOutputReadme, Is.EqualTo(_readmeRewriterTask.OutputReadme));
                Assert.That(_ioHelper.WriteRewrittenReadme, Is.EqualTo(readmeRewriterResult.RewrittenReadme));
            });
        }

        [TestCase(null,nameof(RewriteTagsOptions.All),false)]
        [TestCase("malformed", nameof(RewriteTagsOptions.All), true)]
        [TestCase("RewriteImgTagsForSupportedDomains, RewriteBrTags", "RewriteImgTagsForSupportedDomains, RewriteBrTags", false)]
        public void Should_Use_RewriteTags_Options(string? rewriteTagsOptionsMSBuild, string expectedRewriteTagsOptionsStr, bool expectsLogsWarning)
        {
            var expectedRewriteTagsOptions = RewriteTagsOptionsParser.Parse(expectedRewriteTagsOptionsStr);
            _readmeRewriterTask.RewriteTagsOptions = rewriteTagsOptionsMSBuild;
            var result = ExecuteReadmeExists();

            _mockReadmeRewriter.Verify(readmeRewriter => readmeRewriter.Rewrite(It.IsAny<string>(), It.IsAny<string>(), repositoryUrl, repositoryBranch, expectedRewriteTagsOptions, It.IsAny<RemoveReplaceSettings>()));

            if (expectsLogsWarning)
            {
                Assert.That(
                    _dummyLogBuildEngine.SingleWarningMessage(), 
                    Is.EqualTo($"{rewriteTagsOptionsMSBuild}{expectedRewriteTagsOptions}"));
            }
            else
            {
                Assert.That(_dummyLogBuildEngine.HasEvents<BuildWarningEventArgs>(), Is.False);
            }
        }
    }
}
