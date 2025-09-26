using NugetReadmeGithubRelativeToRaw.Rewriter.Validation;

namespace UnitTests
{
    internal class ReadmeRewriter_Markdown_Tests : ReadmeRewriter_Tests_Base
    {
        [TestCase("dir/fileName.ext","username","reponame","main")]
        [TestCase("dir2/fileName2.ext", "username2", "reponame2", "master")]
        [TestCase("dir/fileName.ext", "username", "reponame", null)] // should default to master
        public void Should_Rewrite_Relative_Markdown_Image(string relativePath, string username, string reponame,string? repoBranch)
        {
            var readmeContent = CreateMarkdownImage(relativePath);
            var repoUrl = CreateRepositoryUrl(username, reponame);
            var expectedRepoBranch = repoBranch ?? "master";
            var expectedRedmeRewritten = CreateMarkdownImage($"https://raw.githubusercontent.com/{username}/{reponame}/{expectedRepoBranch}/{relativePath}");
            var readmeRewritten = ReadmeRewriter.Rewrite(readmeContent, repoUrl, repoBranch)!.RewrittenReadme;
            Assert.That(readmeRewritten, Is.EqualTo(expectedRedmeRewritten));
        }

        [Test]
        public void Should_Not_Rewrite_Relative_Markdown_Image_In_Code_Block()
        {
            var codeBlock = @$"
    ```html
    ${CreateMarkdownImage("dir/file.png")}
    ```
";
            var readmeRewritten = RewriteUsernameReponameMainBranch(codeBlock).RewrittenReadme;

            Assert.That(readmeRewritten, Is.EqualTo(codeBlock));
        }

        [Test]
        public void Should_Not_Rewrite_Absolute_Markdown_Image()
        {
            var readmeContent = CreateMarkdownImage("https://example.com/image.png");

            var readmeRewritten = RewriteUsernameReponameMainBranch(readmeContent).RewrittenReadme;
            
            Assert.That(readmeRewritten, Is.EqualTo(readmeContent));
        }

        [TestCase("https://raw.githubusercontent.com/me/repo/refs/heads/master/dir/file.gif", false)]
        [TestCase("https://untrusted/file.gif", true)]
        public void Should_Report_On_Untrusted_Image_Domains(string imageUrl, bool expectedUntrusted)
        {
            var readmeContent = CreateMarkdownImage(imageUrl);

            var unsupportedImageDomains = RewriteUsernameReponameMainBranch(readmeContent).UnsupportedImageDomains;
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

                Assert.That(RewriteUsernameReponameMainBranch(workflowBadgeMarkdown).UnsupportedImageDomains, Is.Empty);
            });
        }

        [Test]
        public void Should_Rewrite_Reference_Links()
        {
            var readmeContent = @"
![alt][label]

[label]: image.png
";
            var rewrittenReadMe = RewriteUsernameReponameMainBranch(readmeContent).RewrittenReadme;

            var expectedReadme = @"
![alt][label]

[label]: https://raw.githubusercontent.com/username/reponame/main/image.png
";
            Assert.That(rewrittenReadMe, Is.EqualTo(expectedReadme));
        }
    }
}