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
        public void Should_Replace_Relative_Markdown_Image(string relativePath, string username, string reponame,string? repoBranch)
        {
            var readmeContent = CreateMarkdownImage(relativePath);
            var repoUrl = CreateRepositoryUrl(username, reponame);
            repoBranch = repoBranch ?? "master";
            var expectedRedmeRewritten = CreateMarkdownImage($"https://raw.githubusercontent.com/{username}/{reponame}/{repoBranch}/{relativePath}");
            var readmeRewritten = _readmeRewriter.Rewrite(readmeContent, repoUrl, repoBranch)!.RewrittenReadme;
            Assert.That(readmeRewritten, Is.EqualTo(expectedRedmeRewritten));
        }

        [Test]
        public void Should_Not_Replace_Relative_Markdown_Image_In_Code_Block()
        {
            var codeBlock = @$"
    ```html
    ${CreateMarkdownImage("dir/file.png")}
    ```
";
            var readmeRewritten = RewriteUsernameReponame(codeBlock).RewrittenReadme;

            Assert.That(readmeRewritten, Is.EqualTo(codeBlock));
        }

        [Test]
        public void Should_Not_Replace_Absolute_Markdown_Image()
        {
            var readmeContent = CreateMarkdownImage("https://example.com/image.png");

            var readmeRewritten = RewriteUsernameReponame(readmeContent).RewrittenReadme;
            
            Assert.That(readmeRewritten, Is.EqualTo(readmeContent));
        }

        [TestCase("https://raw.githubusercontent.com/me/repo/refs/heads/master/dir/file.gif", false)]
        [TestCase("https://untrusted/file.gif", true)]
        public void Should_Report_On_Untrusted_Image_Domains(string imageUrl, bool expectedUntrusted)
        {
            var readmeContent = CreateMarkdownImage(imageUrl);

            var unsupportedImageDomains = RewriteUsernameReponame(readmeContent).UnsupportedImageDomains;
            if(expectedUntrusted)
            {
                Assert.That(unsupportedImageDomains, Has.Count.EqualTo(1));
                Assert.That(unsupportedImageDomains, Does.Contain("untrusted"));
            }
            else
            {
                Assert.That(unsupportedImageDomains, Is.Empty);
            }
        }

        [Test]
        public void Should_Trust_Github_Badge_Urls()
        {
            var workflowBadgeMarkdown = @"
[![Workflow name](https://github.com/user/repo/actions/workflows/workflowname.yaml/badge.svg)](https://github.com/user/repo/actions/workflows/workflowname.yaml)
";
            Assert.Multiple(() =>
            {
                Assert.That(NugetTrustedImageDomains.Instance.IsImageDomainTrusted("github.com"), Is.False);

                Assert.That(RewriteUsernameReponame(workflowBadgeMarkdown).UnsupportedImageDomains, Is.Empty);
            });
        }

        [Test]
        public void Should_Rewrite_Reference_Links()
        {
            var readmeContent = @"
![alt][label]

[label]: image.png
";
            var rewrittenReadMe = RewriteUsernameReponame(readmeContent).RewrittenReadme;

            var expectedReadme = @"
![alt][label]

[label]: https://raw.githubusercontent.com/username/reponame/main/image.png
";
            Assert.That(rewrittenReadMe, Is.EqualTo(expectedReadme));
        }

        [TestCase(nameof(RewriteTagsOptions.All), true)]
        [TestCase(nameof(RewriteTagsOptions.RewriteImgTagsForSupportedDomains), true)]
        [TestCase(nameof(RewriteTagsOptions.None), false)]
        public void Should_Rewrite_Img_When_RewriteTagsOptions_RewriteImgTagsForSupportedDomains(string rewriteTagsOptions, bool expectsRewrites)
        {
            var repoUrl = CreateRepositoryUrl("username", "reponame");
            var readmeContent = @"<img alt=""alttext"" src=""https://github.com/user/repo/actions/workflows/workflowname.yaml/badge.svg"" />";

            var result =  _readmeRewriter.Rewrite(readmeContent, repoUrl, "main", ParseRewriteTagsOptions(rewriteTagsOptions))!;

            var expectedRewrittenReadme = @"![alttext](https://github.com/user/repo/actions/workflows/workflowname.yaml/badge.svg)";
            var expectedReadme = expectsRewrites ? expectedRewrittenReadme : readmeContent;
            Assert.That(expectedReadme, Is.EqualTo(result.RewrittenReadme));
        }

        [TestCase(nameof(RewriteTagsOptions.All), true)]
        [TestCase(nameof(RewriteTagsOptions.RewriteBrTags), true)]
        [TestCase(nameof(RewriteTagsOptions.None), false)]
        public void Should_Rewrite_Br_When_RewriteTagsOptions_RewriteBrTags(string rewriteTagsOptions, bool expectsRewrites)
        {
            var repoUrl = CreateRepositoryUrl("username", "reponame");
            var readmeContent = @"Line1<br/>";
            var result = _readmeRewriter.Rewrite(readmeContent, repoUrl, "main", ParseRewriteTagsOptions(rewriteTagsOptions))!;

            var expectedRewrittenReadme = "Line1\\";
            var expectedReadme = expectsRewrites ? expectedRewrittenReadme : readmeContent;
            Assert.That(result.RewrittenReadme, Is.EqualTo(expectedReadme));
        }

        [TestCase("<br>")]
        [TestCase("<br />")]
        [TestCase("<BR/>")]
        public void Should_Rewrite_Br_Different_Formats(string br)
        {
            var repoUrl = CreateRepositoryUrl("username", "reponame");
            var readmeContent = $"Line1{br}";
            var result = _readmeRewriter.Rewrite(readmeContent, repoUrl, "main", RewriteTagsOptions.All)!;

            var expectedReadme = "Line1\\";
            Assert.That(result.RewrittenReadme, Is.EqualTo(expectedReadme));
        }

        private RewriteTagsOptions ParseRewriteTagsOptions(string rewriteTagsOptions) => (RewriteTagsOptions)Enum.Parse(typeof(RewriteTagsOptions), rewriteTagsOptions);

        private ReadmeRewriterResult RewriteUsernameReponame(string readmeContent, string branch = "main")
        {
            var repoUrl = CreateRepositoryUrl("username", "reponame");

            return _readmeRewriter.Rewrite(readmeContent, repoUrl, "main")!;
        }

        private static string CreateMarkdownImage(string path) => $"![description]({path})";

        private static string CreateRepositoryUrl(string user, string repo) => $"https://github.com/{user}/{repo}.git";
    }
}