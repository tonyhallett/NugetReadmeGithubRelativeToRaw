using System.Collections.Generic;

namespace NugetReadmeGithubRelativeToRaw.Rewriter
{
    internal interface IMarkdownElementsProcessResult
    {
        IEnumerable<SourceReplacement> SourceReplacements { get; }

        IEnumerable<string> UnsupportedImageDomains { get; }
        
        public IEnumerable<string> MissingReadmeAssets { get; }


    }
}