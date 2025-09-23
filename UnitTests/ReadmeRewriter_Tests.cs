using NugetReadmeGithubRelativeToRaw;

namespace UnitTests
{
    public class ReadmeRewriter_Tests
    {
        private ReadmeRewriter _readmeRewriter;

        [SetUp]
        public void Setup() => _readmeRewriter = new ReadmeRewriter();

        [TestCase("dir/fileName.ext","username","reponame","main")]
        [TestCase("dir2/fileName2.ext", "username2", "reponame2", "master")]
        [TestCase("dir/fileName.ext", "username", "reponame", null)] // should default to master
        public void Should_Replace(string relativePath, string username, string reponame,string? repoBranch)
        {
            var readmeContent = CreateMarkdownImage(relativePath);
            var repoUrl = CreateRepositoryUrl(username, reponame);
            repoBranch = repoBranch ?? "master";
            var expectedRedmeRewritten = CreateMarkdownImage($"https://raw.githubusercontent.com/{username}/{reponame}/{repoBranch}/{relativePath}");
            var readmeRewritten = _readmeRewriter.Rewrite(readmeContent, repoUrl, repoBranch);
            Assert.That(readmeRewritten, Is.EqualTo(expectedRedmeRewritten));
        }

        [Test]
        public void Should_Not_Replace_When_Absolute_Url()
        {
            var absolutePath = "https://example.com/image.png";
            var readmeContent = CreateMarkdownImage(absolutePath);
            var repoUrl = CreateRepositoryUrl("username", "reponame");

            var readmeRewritten = _readmeRewriter.Rewrite(readmeContent, repoUrl, "main");
            
            Assert.That(readmeRewritten, Is.EqualTo(readmeContent));
        }

        private static string CreateMarkdownImage(string path) => $"![description]({path})";

        private static string CreateRepositoryUrl(string user, string repo) => $"https://github.com/{user}/{repo}.git";
    }
}