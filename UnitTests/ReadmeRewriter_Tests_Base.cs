using NugetReadmeGithubRelativeToRaw.Rewriter;

namespace UnitTests
{
    internal abstract class ReadmeRewriter_Tests_Base {
        private ReadmeRewriter? _readmeRewriter;
        private DummyReadmeRelativeFileExists _dummyReadmeRelativeFileExists;
        protected ReadmeRewriter ReadmeRewriter => _readmeRewriter!;
        
        protected DummyReadmeRelativeFileExists DummyReadmeRelativeFileExists => _dummyReadmeRelativeFileExists;

        [SetUp]
        public void Setup()
        {
            _readmeRewriter = new ReadmeRewriter();
            _dummyReadmeRelativeFileExists = new DummyReadmeRelativeFileExists();
        }

        protected ReadmeRewriterResult RewriteUserRepoMainReadMe(string readmeContent, RewriteTagsOptions rewriteTagsOptions = RewriteTagsOptions.All, RemoveReplaceSettings? removeReplaceSettings = null)
        {
            var repoUrl = CreateGitHubRepositoryUrl("username", "reponame");

            return ReadmeRewriter.Rewrite(rewriteTagsOptions, readmeContent, "/readme.md", repoUrl, "main", removeReplaceSettings, _dummyReadmeRelativeFileExists)!;
        }

        protected ReadmeRewriterResult RewriteUserRepoMainReadMe(string readmeContent, string rewriteTagsOptions)
            => RewriteUserRepoMainReadMe(readmeContent,RewriteTagsOptionsParser.Parse(rewriteTagsOptions));

        protected static string CreateMarkdownImage(string path, string imageDescription = "description") => $"![{imageDescription}]({path})";
        
        protected static string CreateMarkdownLink(string path, string alt = "alt") => $"[{alt}]({path})";

        protected static string CreateImage(string alt, string src, bool lowercaseTag = true)
        {
            var imgTag = lowercaseTag ? "img" : "IMG";
            return $"<{imgTag} alt=\"{alt}\" src=\"{src}\" />";
        }

        protected static string CreateGitHubRepositoryUrl(string user, string repo) => $"https://github.com/{user}/{repo}.git";
    }
}