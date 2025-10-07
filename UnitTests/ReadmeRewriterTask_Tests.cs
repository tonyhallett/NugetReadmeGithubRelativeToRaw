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
        private const string projectDirectoryPath = "projectdir";
        private const string removeCommentIdentifiers = "removeCommentIdentifiers";
        private readonly ITaskItem[] removeReplaceTaskItems = [new Mock<ITaskItem>().Object];
        private DummyIOHelper _ioHelper;
        private Mock<IRemoveReplaceSettingsProvider> _mockRemoveReplaceSettingsProvider;
        private Mock<IReadmeRewriter> _mockReadmeRewriter;
        private DummyLogBuildEngine _dummyLogBuildEngine;
        private ReadmeRewriterTask _readmeRewriterTask;
        private TestRemoveReplaceSettingsResult removeReplaceSettingsResult;

        private sealed class TestRemoveReplaceSettingsResult : IRemoveReplaceSettingsResult
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
                ProjectDirectoryPath = projectDirectoryPath,
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

        [TestCase((string?)null, "readme.md")]
        [TestCase("relativeReadme.md", "relativeReadme.md")]
        public void Should_Pass_The_ReadmeRelativeFileExists_To_The_ReadmeRewriter(
            string? readmeRelativePath,
            string expectedReadmeRelativePath
            )
        {
            SetupReadMeRewriter(
                new ReadmeRewriterResult(null, [],[], false, false), 
                RewriteTagsOptions.Error, 
                readmeRelativePath, 
                expectedReadmeRelativePath);
            
            _ = ExecuteReadmeExists();

            _mockReadmeRewriter.Verify(readmeRewriter => readmeRewriter.Rewrite(
                It.IsAny<RewriteTagsOptions>(),
                DummyIOHelper.ReadmeText,
                expectedReadmeRelativePath,
                repositoryUrl,
                It.IsAny<string>(),
                removeReplaceSettingsResult.Settings,
                It.Is<ReadmeRelativeFileExists>(readmeRelativeFileExists => readmeRelativeFileExists.ReadmeRelativePath == expectedReadmeRelativePath && readmeRelativeFileExists.ProjectDirectoryPath == projectDirectoryPath))
            );
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

        [TestCase(true)]
        [TestCase(false)]
        public void Should_ReadmeRewriter_Rewrite_The_Readme_File_When_Exists(bool withSettings)
        {
            if (withSettings)
            {
                removeReplaceSettingsResult.Settings = new RemoveReplaceSettings(null, []);
            }
            SetupReadMeRewriter(new ReadmeRewriterResult(null, [],[], false, false));
            _ = ExecuteReadmeExists();

            _mockReadmeRewriter.VerifyAll();
        }

        private void SetupReadMeRewriter(
            ReadmeRewriterResult readmeRewriterResult, 
            RewriteTagsOptions rewriteTagsOptions = RewriteTagsOptions.Error, 
            string? readmeRelativePath = null,
            string? expectedReadmeRelativePath = null,
            string? expectedRepositoryUrl = null
            )
        {
            _readmeRewriterTask.ReadmeRelativePath = readmeRelativePath;
            _mockReadmeRewriter.Setup(readmeRewriter => readmeRewriter.Rewrite(
                rewriteTagsOptions,
                DummyIOHelper.ReadmeText,
                expectedReadmeRelativePath ?? "readme.md",
                expectedRepositoryUrl ?? repositoryUrl,
                It.IsAny<string>(),
                removeReplaceSettingsResult.Settings, 
                It.IsAny<IReadmeRelativeFileExists>())
            ).Returns(readmeRewriterResult);
        }

        [Test]
        public void Should_Log_Error_When_RepositoryUrl_Cannot_Be_Parsed()
        {
            SetupReadMeRewriter(new ReadmeRewriterResult(null, [],[], false, true));
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
            ReadmeRewriterResult readmeRewriterResult = new ReadmeRewriterResult(null, ["unsupported1", "unsupported2"],[], false, false);
            _mockReadmeRewriter.Setup(readmeRewriter => readmeRewriter.Rewrite(
                It.IsAny<RewriteTagsOptions>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                repositoryUrl,
                It.IsAny<string>(),
                It.IsAny<RemoveReplaceSettings>(), 
                It.IsAny<IReadmeRelativeFileExists>())).Returns(readmeRewriterResult);

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
        public void Should_Log_Error_For_Every_Missing_Readme_Asset()
        {
            string[] missingReadmeAssets = ["/missing1", "/missing2"];
            SetupReadMeRewriter(new ReadmeRewriterResult(null, [], missingReadmeAssets, false, false));

            var result = ExecuteReadmeExists();

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(false));
                foreach (var (missingReadme, error) in missingReadmeAssets.Zip(_dummyLogBuildEngine.ErrorMessages()))
                {
                    Assert.That(error, Is.EqualTo(missingReadme));
                }
            });
        }

        [Test]
        public void Should_Log_Error_For_Unsupported_HTML()
        {
            ReadmeRewriterResult readmeRewriterResult = new ReadmeRewriterResult(null, [], [], true, false);
            _mockReadmeRewriter.Setup(readmeRewriter => readmeRewriter.Rewrite(
                It.IsAny<RewriteTagsOptions>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                repositoryUrl,
                It.IsAny<string>(),
                It.IsAny<RemoveReplaceSettings>(), 
                It.IsAny<IReadmeRelativeFileExists>())).Returns(readmeRewriterResult);

            var result = ExecuteReadmeExists();

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(false));
                Assert.That(_dummyLogBuildEngine.SingleErrorMessage, Is.EqualTo(nameof(IMessageProvider.ReadmeHasUnsupportedHTML)));
            });
        }


        [Test]
        public void Should_Write_Rewritten_Readme_To_OutputReadme()
        {
            _readmeRewriterTask.OutputReadme = "outputReadme.md";
            removeReplaceSettingsResult.Settings = new RemoveReplaceSettings(null, []);
            ReadmeRewriterResult readmeRewriterResult = new ReadmeRewriterResult("rewrittenReadme", [], [], false, false);
            _mockReadmeRewriter.Setup(readmeRewriter => readmeRewriter.Rewrite(
                It.IsAny<RewriteTagsOptions>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                repositoryUrl,
                It.IsAny<string>(),
                removeReplaceSettingsResult.Settings,
                It.IsAny<IReadmeRelativeFileExists>())).Returns(readmeRewriterResult);

            var result = ExecuteReadmeExists();

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(true));
                Assert.That(_ioHelper.WriteOutputReadme, Is.EqualTo(_readmeRewriterTask.OutputReadme));
                Assert.That(_ioHelper.WriteRewrittenReadme, Is.EqualTo(readmeRewriterResult.RewrittenReadme));
            });
        }

        [TestCase(null,nameof(RewriteTagsOptions.Error),false)] // uses the default
        [TestCase("malformed", nameof(RewriteTagsOptions.Error), true)] // uses the default
        [TestCase("RewriteImgTagsForSupportedDomains, RewriteBrTags", "RewriteImgTagsForSupportedDomains, RewriteBrTags", false)]
        public void Should_Use_RewriteTags_Options(string? rewriteTagsOptionsMSBuild, string expectedRewriteTagsOptionsStr, bool expectsLogsWarning)
        {
            var expectedRewriteTagsOptions = RewriteTagsOptionsParser.Parse(expectedRewriteTagsOptionsStr);
            SetupReadMeRewriter(new ReadmeRewriterResult(null, [], [], false, false), expectedRewriteTagsOptions);
            _readmeRewriterTask.RewriteTagsOptions = rewriteTagsOptionsMSBuild;
            var result = ExecuteReadmeExists();

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

        [Test]
        public void Should_Log_Errors_From_RemoveReplaceSettingsProvider()
        {
            _mockRemoveReplaceSettingsProvider.Setup(removeReplaceSettingsProvider => removeReplaceSettingsProvider.Provide(It.IsAny<ITaskItem[]?>(),It.IsAny<string?>()))
                .Returns(new TestRemoveReplaceSettingsResult
                {
                    Errors = ["error1"]
                });
            ExecuteReadmeExists();

            Assert.That(_dummyLogBuildEngine.SingleErrorMessage(),Is.EqualTo("error1"));
        }

        [Test]
        public void Should_Use_ReadmeRepositoryUrl_If_Provided()
        {
            _readmeRewriterTask.ReadmeRepositoryUrl = "readmeRepositoryUrl";

            SetupReadMeRewriter(
                new ReadmeRewriterResult(null, [], [], false, false),
                RewriteTagsOptions.Error, 
                null,
                null,
                _readmeRewriterTask.ReadmeRepositoryUrl);
            _ = ExecuteReadmeExists();
        }

        [Test]
        public void Should_Prefer_RepositoryRef_For_Ref() 
            => RefTest("repositoryRef", "repositoryCommit", "repositoryBranch", "repositoryRef");

        [Test]
        public void Should_Prefer_RepositoryCommit_For_Ref_If_RepositoryRef_Null()
            => RefTest(null, "repositoryCommit", "repositoryBranch", "repositoryCommit");

        [Test]
        public void Should_Prefer_RepositoryBranch_For_Ref_If_RepositoryRef_And_RepositoryCommit_Null() 
            => RefTest(null, null, "repositoryBranch", "repositoryBranch");

        [Test]
        public void Should_Default_Ref_To_Master_If_RepositoryRef_RepositoryCommit_And_RepositoryBranch_Null() 
            => RefTest(null, null, null, "master");

        private void RefTest(string? repositoryRef, string? repositoryCommit, string? repositoryBranch, string expectedRef)
        {
            _readmeRewriterTask.RepositoryRef = repositoryRef;
            _readmeRewriterTask.RepositoryCommit = repositoryCommit;
            _readmeRewriterTask.RepositoryBranch = repositoryBranch;

            _mockReadmeRewriter.Setup(readmeRewriter => readmeRewriter.Rewrite(
                It.IsAny<RewriteTagsOptions>(),
                DummyIOHelper.ReadmeText,
                It.IsAny<string>(),
                It.IsAny<string>(),
                expectedRef,
               It.IsAny<RemoveReplaceSettings?>(),
                It.IsAny<IReadmeRelativeFileExists>())
            ).Returns(new ReadmeRewriterResult(null, [], [], false, false));

            _ = ExecuteReadmeExists();

        }
    }
}
