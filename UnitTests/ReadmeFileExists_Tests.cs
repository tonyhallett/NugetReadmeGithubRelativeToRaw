using NugetReadmeGithubRelativeToRaw;

namespace UnitTests
{
    internal class ReadmeFileExists_Tests
    {
        private DirectoryInfo _tempProjectDirectory;

        [SetUp]
        public void Setup()
        {
            _tempProjectDirectory = Directory.CreateTempSubdirectory();
        }

        private ReadmeRelativeFileExists Initialize(string readmeRelativePath)
        {
            return new ReadmeRelativeFileExists(
                _tempProjectDirectory.FullName,
                readmeRelativePath);
        }


        [TestCase(true)]
        [TestCase(false)]
        public void Should_Work_Relative_To_Repo_Root(bool exists)
        {
            var readmeRelativeFileExists = Initialize("readmedir/readme.md");
            var filePath = Path.Combine(_tempProjectDirectory.FullName, "file.txt");
            if (exists)
            {
                File.WriteAllText(filePath, "test");
            }
            
            Assert.That(readmeRelativeFileExists.Exists("/file.txt"), Is.EqualTo(exists));
        }

        [TestCase("./")]
        [TestCase("")]
        public void Should_Work_Relative_To_Readme(string prefix)
        {
            var readmeRelativeFileExists = Initialize("readmedir/readme.md");
            var readmeDirectoryPath = Path.Combine(_tempProjectDirectory.FullName, "readmedir");
            Directory.CreateDirectory(readmeDirectoryPath);
            var filePath = Path.Combine(readmeDirectoryPath, "file.txt");
            File.WriteAllText(filePath, "test");

            Assert.That(readmeRelativeFileExists.Exists($"{prefix}file.txt"), Is.True);
        }

        [Test]
        public void Should_Work_Relative_To_Readme_Parent()
        {
            var readmeRelativeFileExists = Initialize("parent/readmedir/readme.md");
            var parentDirectoryPath = Path.Combine(_tempProjectDirectory.FullName, "parent");
            Directory.CreateDirectory(parentDirectoryPath);
            var filePath = Path.Combine(parentDirectoryPath, "file.txt");
            File.WriteAllText(filePath, "test");

            Assert.That(readmeRelativeFileExists.Exists($"../file.txt"), Is.True);
        }


        [TearDown] 
        public void Teardown() {
            _tempProjectDirectory.Delete(true);
        }
    }
}
