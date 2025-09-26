using NugetReadmeGithubRelativeToRaw.Rewriter;

namespace UnitTests
{
    internal class GithubUrlHelper_GetGithubAbsoluteUrl_Tests
    {
        [Test]
        public void Should_Return_Null_If_Null_Url()
        {
            var gitHubUrlHelper = new GitHubUrlHelper();
            var url = gitHubUrlHelper.GetGithubAbsoluteUrl(null, OwnerRepoRefReadmePath.Create("https://github.com/owner/repo", "main", "/readme.md")!, false);
            Assert.IsNull(url);
        }

        [TestCase(@"http://www.example.com")]
        [TestCase(@"//www.example.com")]
        [TestCase(@"//example.com")]
        public void Should_Return_Null_For_Should_Return_Null_If_Null_Url_If_Not_Relative(string absoluteUrl)
        {
            var gitHubUrlHelper = new GitHubUrlHelper();
            var url = gitHubUrlHelper.GetGithubAbsoluteUrl(absoluteUrl, OwnerRepoRefReadmePath.Create("https://github.com/owner/repo", "main", "/readme.md")!, false);
            Assert.IsNull(url);
        }

        [Test]
        public void Should_Append_If_Url_Is_Relative_To_The_Repo()
        {
            var gitHubUrlHelper = new GitHubUrlHelper();
            var url = gitHubUrlHelper.GetGithubAbsoluteUrl("/reporelative.md", OwnerRepoRefReadmePath.Create("https://github.com/owner/repo", "main", "/docs/readme.md")!, false);

            Assert.That(url, Is.EqualTo("https://github.com/owner/repo/blob/main/reporelative.md"));
        }

        [TestCase("indocs.md", "https://github.com/owner/repo/blob/main/docs/indocs.md")]
        [TestCase("./indocs.md", "https://github.com/owner/repo/blob/main/docs/indocs.md")]
        [TestCase("../inreporoot.md", "https://github.com/owner/repo/blob/main/inreporoot.md")]
        public void Should_Be_Relative_To_The_Readme_When_Does_Not_Start_With_Forward_Slash(string relativeUrl, string expectedUrl)
        {
            var gitHubUrlHelper = new GitHubUrlHelper();
            var url = gitHubUrlHelper.GetGithubAbsoluteUrl(relativeUrl, OwnerRepoRefReadmePath.Create("https://github.com/owner/repo", "main", "/docs/readme.md")!, false);

            Assert.That(url, Is.EqualTo(expectedUrl));
        }
    }
}
