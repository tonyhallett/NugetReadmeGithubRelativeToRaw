using NugetReadmeGithubRelativeToRaw.Rewriter;

namespace UnitTests
{
    internal class RepoPaths_Tests
    {
        [Test]
        public void Should_Be_Null_If_Not_Github_Or_Gitlab()
        {
            Assert.That(RepoPaths.Create("https://notgithub.com/", "master", "readme.md"), Is.Null);
        }

        [Test]
        public void Should_Be_Null_If_Not_Github_Owner_Repo()
        {
            Assert.That(RepoPaths.Create("https://github.com/owner", "master", "readme.md"), Is.Null);
        }

        [Test]
        public void Should_Have_The_Relative_Readme_Path()
        {
            Assert.That(RepoPaths.Create("https://github.com/owner/repo.git", "master", "readme.md")!.ReadmeRelativePath, Is.EqualTo("readme.md"));
        }

        public static object?[] ImageLinkBasePathCases =
        [
            new object?[] { "https://github.com/owner/repo.git", "master", "https://github.com/owner/repo/blob/master", "https://raw.githubusercontent.com/owner/repo/master" },
            new object?[] { "https://github.com/owner/repo", "master", "https://github.com/owner/repo/blob/master", "https://raw.githubusercontent.com/owner/repo/master" },
            new object?[] { "https://github.com/owner/repo/", "master", "https://github.com/owner/repo/blob/master", "https://raw.githubusercontent.com/owner/repo/master" },
            new object[] { "https://github.com/owner/repo.git", "main", "https://github.com/owner/repo/blob/main", "https://raw.githubusercontent.com/owner/repo/main" },
            new object[] { "https://gitlab.com/group/subgroup/repo.git", "main", "https://gitlab.com/group/subgroup/repo/-/blob/main", "https://gitlab.com/group/subgroup/repo/-/raw/main" },
            new object[] { "https://gitlab.com/user/repo.git", "main", "https://gitlab.com/user/repo/-/blob/main", "https://gitlab.com/user/repo/-/raw/main" }
        ];

        [TestCaseSource(nameof(ImageLinkBasePathCases))]
        public void Should_Have_Correct_Image_Link_Base_Paths(
            string repoUrl, 
            string @ref, 
            string expectedLinkBasePath,
            string expectedImageBasePath)
        {
            var repoPaths = RepoPaths.Create(repoUrl, @ref, "readme.md");
            Assert.Multiple(() =>
            {
                Assert.That(repoPaths!.LinkBasePath, Is.EqualTo(expectedLinkBasePath));
                Assert.That(repoPaths!.ImageBasePath, Is.EqualTo(expectedImageBasePath));
            });
        }
    }
}
