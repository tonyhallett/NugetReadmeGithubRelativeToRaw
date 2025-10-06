using NugetReadmeGithubRelativeToRaw.Rewriter;

namespace UnitTests
{
    internal class OwnerRepoRefReadmePath_Tests
    {
        [Test]
        public void Should_Be_Null_If_Not_Github()
        {
            Assert.That(OwnerRepoRefReadmePath.Create("https://notgithub.com/", null, "readme.md"), Is.Null);
        }

        [Test]
        public void Should_Be_Null_If_Not_Owner_Repo()
        {
            Assert.That(OwnerRepoRefReadmePath.Create("https://github.com/owner", null, "readme.md"), Is.Null);
        }

        [Test]
        public void Should_Have_OwnerRepoUrlPart()
        {
            OwnerRepoTest(false);
        }

        [Test]
        public void Should_Accept_Git_Github_Url()
        {
            OwnerRepoTest(true);
        }

        [TestCase((string?)null, "master")]
        [TestCase("main", "main")]
        public void Should_Default_Ref_To_Master(string? githubRef, string expected)
        {
            Assert.That(OwnerRepoRefReadmePath.Create("https://github.com/owner/repo", githubRef, "readme.md")!.Ref, Is.EqualTo(expected));

        }

        private static void OwnerRepoTest(bool isGitUrl)
        {
            var urlSuffix = isGitUrl ? ".git" : "";
            var ownerRepoUrlPart = OwnerRepoRefReadmePath.Create("https://github.com/owner/repo" + urlSuffix, null, "readme.md")!.OwnerRepoUrlPart;
            Assert.That(ownerRepoUrlPart, Is.EqualTo("owner/repo"));
        }
    }
}
