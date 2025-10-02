using NugetReadmeGithubRelativeToRaw.Rewriter;
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
            var readmeRewritten = ReadmeRewriter.Rewrite(RewriteTagsOptions.None, readmeContent, "/readme.md", repoUrl, repoRef)!.RewrittenReadme;
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
            var readmeRewritten = RewriteUserRepoMainReadMe(codeBlock).RewrittenReadme;

            Assert.That(readmeRewritten, Is.EqualTo(codeBlock));
        }

        [TestCase("https://raw.githubusercontent.com/me/repo/refs/heads/master/dir/file.gif", false)]
        [TestCase("https://untrusted/file.gif", true)]
        public void Should_Report_On_Untrusted_Image_Domains(string imageUrl, bool expectedUntrusted)
        {
            var readmeContent = CreateMarkdownImage(imageUrl);

            var result = RewriteUserRepoMainReadMe(readmeContent);
            var unsupportedImageDomains = result.UnsupportedImageDomains;
            if (expectedUntrusted)
            {
                Assert.Multiple(() =>
                {
                    Assert.That(result.RewrittenReadme, Is.Null);
                    Assert.That(unsupportedImageDomains.Single(), Is.EqualTo("untrusted"));
                });
            }
            else
            {
                Assert.Multiple(() =>
                {
                    Assert.That(result.RewrittenReadme, Is.EqualTo(readmeContent));
                    Assert.That(unsupportedImageDomains, Is.Empty);
                });
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

                Assert.That(RewriteUserRepoMainReadMe(workflowBadgeMarkdown).UnsupportedImageDomains, Is.Empty);
            });
        }

        [Test]
        public void Should_Rewrite_Reference_Image_Links()
        {
            var readmeContent = @"
![alt][label]

[label]: image.png
";
            var rewrittenReadMe = RewriteUserRepoMainReadMe(readmeContent).RewrittenReadme;

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
            var rewrittenReadMe = RewriteUserRepoMainReadMe(readmeContent).RewrittenReadme;

            var expectedReadme = @"
[alt][label]

[label]: https://github.com/username/reponame/blob/main/page.md
";
            Assert.That(rewrittenReadMe, Is.EqualTo(expectedReadme));
        }

        [Test]
        public void Should_Replace_Github_Readme_Marker()
        {
            var readMeContent = @"
Intro
# Github only
";
            var githubReplacementLine = $"For full details visit [GitHub]({ReadmeRewriter.GithubReadmeMarker})";
            RemovalOrReplacement githubReplacement = new RemovalOrReplacement(CommentOrRegex.Regex, "# Github only", null, githubReplacementLine);
            var removeReplaceSettings = new RemoveReplaceSettings(null, [githubReplacement]);

            var rewrittenReadMe = RewriteUserRepoMainReadMe(readMeContent, RewriteTagsOptions.None,removeReplaceSettings).RewrittenReadme;

            var expectedReadMeContent = @"
Intro
For full details visit [GitHub](https://github.com/username/reponame/blob/main/readme.md)";

            Assert.That(rewrittenReadMe, Is.EqualTo(expectedReadMeContent));
        }
    }
}