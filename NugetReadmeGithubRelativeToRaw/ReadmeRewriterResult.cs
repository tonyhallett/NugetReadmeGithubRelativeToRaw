using System.Collections.Generic;

namespace NugetReadmeGithubRelativeToRaw
{
    internal class ReadmeRewriterResult
    {
        public ReadmeRewriterResult(string rewrittenReadme, IEnumerable<string> unsupportedImageDomains)
        {
            RewrittenReadme = rewrittenReadme;
            UnsupportedImageDomains = new List<string>(unsupportedImageDomains);
        }

        public string RewrittenReadme { get; }

        public IReadOnlyList<string> UnsupportedImageDomains { get; }
    }
}
