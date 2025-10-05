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
            var result = RewriteUserRepoMainReadMe(readmeContent, rewriteTagsOptions);

            var expectedRewrittenReadme = CreateMarkdownImage("https://github.com/user/repo/actions/workflows/workflowname.yaml/badge.svg", "alttext");
            var expectedReadme = expectsRewrites ? expectedRewrittenReadme : readmeContent;
            Assert.Multiple(() =>
            {
                Assert.That(result.HasUnsupportedHTML, Is.False);
                Assert.That(expectedReadme, Is.EqualTo(result.RewrittenReadme));
                Assert.That(result.UnsupportedImageDomains, Is.Empty);
            });
        }

        [Test]
        public void Should_Rewrite_Relative_Img()
        {
            var readmeContent = CreateImage("alttext", "relative.png");
            var result = RewriteUserRepoMainReadMe(readmeContent);

            var expectedReadme = CreateMarkdownImage("https://raw.githubusercontent.com/username/reponame/main/relative.png", "alttext");
            Assert.Multiple(() =>
            {
                Assert.That(result.HasUnsupportedHTML, Is.False);
                Assert.That(expectedReadme, Is.EqualTo(result.RewrittenReadme));
                Assert.That(result.UnsupportedImageDomains, Is.Empty);
            });
        }

        [Test]
        public void Should_Not_Rewrite_Imgs_For_Unsupported_Domains()
        {
            var unsupportedImage1 = CreateImage("altext", "https://unsupported.com/a.png");
            var unsupportedImage2= CreateImage("altext", "https://unsupported2.com/a.png");
            var readmeContent = @$"
{unsupportedImage1}

{unsupportedImage2}
";
            
            var result = RewriteUserRepoMainReadMe(readmeContent);

            Assert.Multiple(() =>
            {
                Assert.That(result.HasUnsupportedHTML, Is.False);
                Assert.That(result.RewrittenReadme, Is.Null);
                Assert.That(result.UnsupportedImageDomains[0], Is.EqualTo("unsupported.com"));
                Assert.That(result.UnsupportedImageDomains[1], Is.EqualTo("unsupported2.com"));
            });
        }

        [TestCase(nameof(RewriteTagsOptions.All), true)]
        [TestCase(nameof(RewriteTagsOptions.RewriteBrTags), true)]
        [TestCase(nameof(RewriteTagsOptions.None), false)]
        public void Should_Rewrite_Br_When_RewriteTagsOptions_RewriteBrTags(string rewriteTagsOptions, bool expectsRewrites)
        {
            var readmeContent = @"Line1<br/>";
            var result = RewriteUserRepoMainReadMe(readmeContent, rewriteTagsOptions);

            var expectedRewrittenReadme = "Line1\\";
            var expectedReadme = expectsRewrites ? expectedRewrittenReadme : readmeContent;

            Assert.Multiple(() =>
            {
                Assert.That(result.HasUnsupportedHTML, Is.False);
                Assert.That(result.RewrittenReadme, Is.EqualTo(expectedReadme));
            });
            
        }

        [TestCase("<br>")]
        [TestCase("<br />")]
        [TestCase("<BR/>")]
        public void Should_Rewrite_Br_Different_Formats(string br)
        {
            var readmeContent = $"Line1{br}";
            var result = RewriteUserRepoMainReadMe(readmeContent, RewriteTagsOptions.All);

            var expectedReadme = "Line1\\";

            Assert.Multiple(() =>
            {
                Assert.That(result.RewrittenReadme, Is.EqualTo(expectedReadme));
                Assert.That(result.HasUnsupportedHTML, Is.False);
                Assert.That(result.RewrittenReadme, Is.EqualTo(expectedReadme));
            });
        }

        [TestCase(nameof(RewriteTagsOptions.All), true, true)]
        [TestCase(nameof(RewriteTagsOptions.RewriteATags), true, false)]
        [TestCase(nameof(RewriteTagsOptions.None), false, true)]
        public void Should_Rewrite_A_Tag_When_RewriteTagsOptions_RewriteATags_Relative(string rewriteTagsOptions, bool expectsRewrites, bool lowercaseTag)
        {
            var aTag = lowercaseTag ? "a" : "A";
            var readmeContent = @$"<{aTag} href=""abc.html"">TextContent</{aTag}>";

            var result = RewriteUserRepoMainReadMe(readmeContent, rewriteTagsOptions);
            
            var expectedRewrittenReadme = @"[TextContent](https://github.com/username/reponame/blob/main/abc.html)";
            var expectedReadme = expectsRewrites ? expectedRewrittenReadme : readmeContent;
            Assert.Multiple(() =>
            {
                Assert.That(result.HasUnsupportedHTML, Is.False);
                Assert.That(expectedReadme, Is.EqualTo(result.RewrittenReadme));
            });
        }

        [Test]
        public void Should_Rewrite_A_Tag_Absolute()
        {
            var readmeContent = @$"<a href=""https://example.org/some-page"">Some page</a>";

            var result = RewriteUserRepoMainReadMe(readmeContent);

            var expectedReadme = @"[Some page](https://example.org/some-page)";
            Assert.Multiple(() =>
            {
                Assert.That(result.RewrittenReadme, Is.EqualTo(expectedReadme));
                Assert.That(result.UnsupportedImageDomains, Is.Empty);
                Assert.That(result.HasUnsupportedHTML, Is.False);
            });
        }

        [Test]
        public void Should_Have_UnsupportedHTML_When_RewriteTagsOptions_Error_And_HTML_In_Readme()
        {
            var result = RewriteUserRepoMainReadMe("<br/>", RewriteTagsOptions.Error);
            Assert.That(result.HasUnsupportedHTML, Is.True);
        }
    }
}