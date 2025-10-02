using System.Collections.Generic;

namespace NugetReadmeGithubRelativeToRaw.Rewriter
{
    internal class ReadmeRewriterResult
    {
        public ReadmeRewriterResult(
            string? rewrittenReadme, 
            IEnumerable<string> unsupportedImageDomains, 
            bool hasUnsupportedHTML,
            bool unsupportedRepo)
        {
            RewrittenReadme = rewrittenReadme;
            UnsupportedImageDomains = new List<string>(unsupportedImageDomains);
            HasUnsupportedHTML = hasUnsupportedHTML;
            UnsupportedRepo = unsupportedRepo;
        }

        public string? RewrittenReadme { get; }

        public IReadOnlyList<string> UnsupportedImageDomains { get; }

        public bool HasUnsupportedHTML { get; }

        public bool UnsupportedRepo { get; }
    }
}
