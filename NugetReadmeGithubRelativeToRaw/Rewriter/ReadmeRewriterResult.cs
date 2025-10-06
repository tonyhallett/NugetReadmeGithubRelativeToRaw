using System.Collections.Generic;

namespace NugetReadmeGithubRelativeToRaw.Rewriter
{
    internal class ReadmeRewriterResult
    {
        public ReadmeRewriterResult(
            string? rewrittenReadme, 
            IEnumerable<string> unsupportedImageDomains,
            IEnumerable<string> missingReadmeAssets,
            bool hasUnsupportedHTML,
            bool unsupportedRepo)
        {
            RewrittenReadme = rewrittenReadme;
            UnsupportedImageDomains = unsupportedImageDomains;
            MissingReadmeAssets = missingReadmeAssets;
            HasUnsupportedHTML = hasUnsupportedHTML;
            UnsupportedRepo = unsupportedRepo;
        }

        public string? RewrittenReadme { get; }

        public IEnumerable<string> UnsupportedImageDomains { get; }

        public IEnumerable<string> MissingReadmeAssets { get; }

        public bool HasUnsupportedHTML { get; }

        public bool UnsupportedRepo { get; }
    }
}
