using NugetReadmeGithubRelativeToRaw.Rewriter.Validation;

namespace UnitTests
{
    internal class ReadmeRewriter_Markdown_Tests : ReadmeRewriter_Tests_Base
    {
        [TestCase("dir/fileName.ext","username","reponame","main")]
        [TestCase("dir2/fileName2.ext", "username2", "reponame2", "master")]
        [TestCase("dir/fileName.ext", "username", "reponame", null)] // should default to master
        public void Should_Rewrite_Relative_Markdown_Image(string relativePath, string username, string reponame,string? repoRef)
        {
            var readmeContent = CreateMarkdownImage(relativePath);
            var repoUrl = CreateRepositoryUrl(username, reponame);
            var expectedRepoRef = repoRef ?? "master";
            var expectedRedmeRewritten = CreateMarkdownImage($"https://raw.githubusercontent.com/{username}/{reponame}/{expectedRepoRef}/{relativePath}");
            var readmeRewritten = ReadmeRewriter.Rewrite(readmeContent, "/readme.md", repoUrl, repoRef)!.RewrittenReadme;
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
            var readmeRewritten = RewriteUseRepoMainReadMe(codeBlock).RewrittenReadme;

            Assert.That(readmeRewritten, Is.EqualTo(codeBlock));
        }

        [Test]
        public void Should_Not_Rewrite_Absolute_Markdown_Image()
        {
            var readmeContent = CreateMarkdownImage("https://example.com/image.png");

            var readmeRewritten = RewriteUseRepoMainReadMe(readmeContent).RewrittenReadme;
            
            Assert.That(readmeRewritten, Is.EqualTo(readmeContent));
        }

        [TestCase("https://raw.githubusercontent.com/me/repo/refs/heads/master/dir/file.gif", false)]
        [TestCase("https://untrusted/file.gif", true)]
        public void Should_Report_On_Untrusted_Image_Domains(string imageUrl, bool expectedUntrusted)
        {
            var readmeContent = CreateMarkdownImage(imageUrl);

            var unsupportedImageDomains = RewriteUseRepoMainReadMe(readmeContent).UnsupportedImageDomains;
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
        public void Should_Trust_GitHub_Badge_Urls()
        {
            var workflowBadgeMarkdown = @"
[![Workflow name](https://github.com/user/repo/actions/workflows/workflowname.yaml/badge.svg)](https://github.com/user/repo/actions/workflows/workflowname.yaml)
";
            Assert.Multiple(() =>
            {
                Assert.That(NuGetTrustedImageDomains.Instance.IsImageDomainTrusted("github.com"), Is.False);

                Assert.That(RewriteUseRepoMainReadMe(workflowBadgeMarkdown).UnsupportedImageDomains, Is.Empty);
            });
        }

        [Test]
        public void Should_Rewrite_Reference_Image_Links()
        {
            var readmeContent = @"
![alt][label]

[label]: image.png
";
            var rewrittenReadMe = RewriteUseRepoMainReadMe(readmeContent).RewrittenReadme;

            var expectedReadme = @"
![alt][label]

[label]: https://raw.githubusercontent.com/username/reponame/main/image.png
";
            Assert.That(rewrittenReadMe, Is.EqualTo(expectedReadme));
        }

        [Test]
        public void Should_Rewrite_Reference_Links()
        {
            var readmeContent = @"
[alt][label]

[label]: page.md
";
            var rewrittenReadMe = RewriteUseRepoMainReadMe(readmeContent).RewrittenReadme;

            var expectedReadme = @"
[alt][label]

[label]: https://github.com/username/reponame/blob/main/page.md
";
            Assert.That(rewrittenReadMe, Is.EqualTo(expectedReadme));
        }
    }
}