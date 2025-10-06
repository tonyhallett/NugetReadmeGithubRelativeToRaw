using Moq;
using NugetReadmeGithubRelativeToRaw;

namespace UnitTests
{
    internal class ReadmeFileExists_Tests
    {
        private Mock<IIOHelper> _mockIOHelper;
        private readonly ReadmeRelativeFileExistsFactory readmeFileExistsFactory = new ReadmeRelativeFileExistsFactory();

        [SetUp]
        public void Setup()
        {
            _mockIOHelper = new Mock<IIOHelper>(MockBehavior.Strict);
        }

        [Test]
        public void Should_Work_Repo_Relatively()
        {
            var readmeFileExists = readmeFileExistsFactory.Create("C:\\Users\\tonyh\\Repos\\ProjectDir", "subdir/readme.md", _mockIOHelper.Object);

            _mockIOHelper.Setup(ioHelper => ioHelper.FileExists("C:\\Users\\tonyh\\Repos\\ProjectDir\\repoRelative.png")).Returns(true);

            Assert.That(readmeFileExists.Exists("/repoRelative.png"), Is.True);
        }

        [Test]
        public void Should_Work_Relative_To_Readme_Forwardslash()
        {
            var readmeFileExists = readmeFileExistsFactory.Create("C:\\Users\\tonyh\\Repos\\ProjectDir", "subdir/readme.md", _mockIOHelper.Object);

            _mockIOHelper.Setup(ioHelper => ioHelper.FileExists("C:\\Users\\tonyh\\Repos\\ProjectDir\\subdir\\readmeRelative.png")).Returns(true);

            Assert.That(readmeFileExists.Exists("readmeRelative.png"), Is.True);
        }

        [Test]
        public void Should_Work_Relative_To_Readme_Backslash()
        {
            var readmeFileExists = readmeFileExistsFactory.Create("C:\\Users\\tonyh\\Repos\\ProjectDir", "subdir\\readme.md", _mockIOHelper.Object);

            _mockIOHelper.Setup(ioHelper => ioHelper.FileExists("C:\\Users\\tonyh\\Repos\\ProjectDir\\subdir\\readmeRelative.png")).Returns(true);

            Assert.That(readmeFileExists.Exists("readmeRelative.png"), Is.True);
        }

        // next ./ and ../ tests
    }
}
