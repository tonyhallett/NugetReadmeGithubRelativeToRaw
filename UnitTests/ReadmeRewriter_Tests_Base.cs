using NugetReadmeGithubRelativeToRaw.Rewriter;

namespace UnitTests
{
    internal abstract class ReadmeRewriter_Tests_Base {
        private ReadmeRewriter? _readmeRewriter;
        protected ReadmeRewriter ReadmeRewriter => _readmeRewriter!;

        [SetUp]
        public void Setup() => _readmeRewriter = new ReadmeRewriter();

        protected static RewriteTagsOptions ParseRewriteTagsOptions(string rewriteTagsOptions) => (RewriteTagsOptions)Enum.Parse(typeof(RewriteTagsOptions), rewriteTagsOptions);

        protected ReadmeRewriterResult RewriteUsernameReponameMainBranch(string readmeContent, RewriteTagsOptions rewriteTagsOptions = RewriteTagsOptions.All)
        {
            var repoUrl = CreateRepositoryUrl("username", "reponame");

            return ReadmeRewriter.Rewrite(readmeContent, repoUrl, "main", rewriteTagsOptions)!;
        }

        protected static string CreateMarkdownImage(string path, string imageDescription = "description") => $"![{imageDescription}]({path})";

        protected static string CreateImage(string alt, string src, bool lowercaseTag = true)
        {
            var imgTag = lowercaseTag ? "img" : "IMG";
            return $"<{imgTag} alt=\"{alt}\" src=\"{src}\" />";
        }

        protected static string CreateRepositoryUrl(string user, string repo) => $"https://github.com/{user}/{repo}.git";
    }
}