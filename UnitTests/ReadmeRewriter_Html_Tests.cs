using NugetReadmeGithubRelativeToRaw.Rewriter;

namespace UnitTests
{
    internal class ReadmeRewriter_Html_Tests : ReadmeRewriter_Tests_Base
    {
        [TestCase(nameof(RewriteTagsOptions.All), true, true)]
        [TestCase(nameof(RewriteTagsOptions.RewriteImgTagsForSupportedDomains), true, false)]
        [TestCase(nameof(RewriteTagsOptions.None), false, false)]
        public void Should_Rewrite_Img_When_RewriteTagsOptions_RewriteImgTagsForSupportedDomains(string rewriteTagsOptions, bool expectsRewrites, bool lowercaseTag)
        {
            var readmeContent = CreateImage("alttext", "https://github.com/user/repo/actions/workflows/workflowname.yaml/badge.svg", lowercaseTag);
            var result = RewriteUseRepoMainReadMe(readmeContent, rewriteTagsOptions);

            var expectedRewrittenReadme = CreateMarkdownImage("https://github.com/user/repo/actions/workflows/workflowname.yaml/badge.svg", "alttext");
            var expectedReadme = expectsRewrites ? expectedRewrittenReadme : readmeContent;
            Assert.Multiple(() =>
            {
                Assert.That(expectedReadme, Is.EqualTo(result.RewrittenReadme));
                Assert.That(result.UnsupportedImageDomains, Is.Empty);
            });
        }

        [Test]
        public void Should_Rewrite_Relative_Img()
        {
            var readmeContent = CreateImage("alttext", "relative.png");
            var result = RewriteUseRepoMainReadMe(readmeContent);

            var expectedReadme = CreateMarkdownImage("https://raw.githubusercontent.com/username/reponame/main/relative.png", "alttext");
            Assert.Multiple(() =>
            {
                Assert.That(expectedReadme, Is.EqualTo(result.RewrittenReadme));
                Assert.That(result.UnsupportedImageDomains, Is.Empty);
            });
        }

        [Test]
        public void Should_Not_Rewrite_Img_For_Unsupported_Domains()
        {
            var readmeContent = CreateImage("altext", "https://unsupported.com/a.png");
            var result = RewriteUseRepoMainReadMe(readmeContent);

            Assert.Multiple(() =>
            {
                Assert.That(readmeContent, Is.EqualTo(result.RewrittenReadme));
                Assert.That(result.UnsupportedImageDomains, Has.Count.EqualTo(1));
            });
            Assert.That(result.UnsupportedImageDomains, Does.Contain("unsupported.com"));
        }

        [TestCase(nameof(RewriteTagsOptions.All), true)]
        [TestCase(nameof(RewriteTagsOptions.RewriteBrTags), true)]
        [TestCase(nameof(RewriteTagsOptions.None), false)]
        public void Should_Rewrite_Br_When_RewriteTagsOptions_RewriteBrTags(string rewriteTagsOptions, bool expectsRewrites)
        {
            var readmeContent = @"Line1<br/>";
            var result = RewriteUseRepoMainReadMe(readmeContent, rewriteTagsOptions);

            var expectedRewrittenReadme = "Line1\\";
            var expectedReadme = expectsRewrites ? expectedRewrittenReadme : readmeContent;
            Assert.That(result.RewrittenReadme, Is.EqualTo(expectedReadme));
        }

        [TestCase("<br>")]
        [TestCase("<br />")]
        [TestCase("<BR/>")]
        public void Should_Rewrite_Br_Different_Formats(string br)
        {
            var readmeContent = $"Line1{br}";
            var result = RewriteUseRepoMainReadMe(readmeContent, RewriteTagsOptions.All);

            var expectedReadme = "Line1\\";
            Assert.That(result.RewrittenReadme, Is.EqualTo(expectedReadme));
        }

        [TestCase(nameof(RewriteTagsOptions.All), true, true)]
        [TestCase(nameof(RewriteTagsOptions.RewriteATags), true, false)]
        [TestCase(nameof(RewriteTagsOptions.None), false, true)]
        public void Should_Rewrite_A_Tag_When_RewriteTagsOptions_RewriteATags_Relative(string rewriteTagsOptions, bool expectsRewrites, bool lowercaseTag)
        {
            var aTag = lowercaseTag ? "a" : "A";
            var readmeContent = @$"<{aTag} href=""abc.html"">TextContent</{aTag}>";

            var result = RewriteUseRepoMainReadMe(readmeContent, rewriteTagsOptions);
            
            var expectedRewrittenReadme = @"[TextContent](https://github.com/username/reponame/blob/main/abc.html)";
            var expectedReadme = expectsRewrites ? expectedRewrittenReadme : readmeContent;
            Assert.That(expectedReadme, Is.EqualTo(result.RewrittenReadme));
        }

        [Test]
        public void Should_Rewrite_A_Tag_Absolute()
        {
            var readmeContent = @$"<a href=""https://example.org/some-page"">Some page</a>";

            var result = RewriteUseRepoMainReadMe(readmeContent);

            var expectedReadme = @"[Some page](https://example.org/some-page)";
            Assert.Multiple(() =>
            {
                Assert.That(result.RewrittenReadme, Is.EqualTo(expectedReadme));
                Assert.That(result.UnsupportedImageDomains, Is.Empty);
            });
        }
    }
}