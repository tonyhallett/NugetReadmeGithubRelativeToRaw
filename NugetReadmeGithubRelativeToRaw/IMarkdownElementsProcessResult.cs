using System.Collections.Generic;

namespace NugetReadmeGithubRelativeToRaw
{
    internal interface IMarkdownElementsProcessResult
    {
        IEnumerable<SourceReplacement> SourceReplacements { get; }
        IEnumerable<string> UnsupportedImageDomains { get; }
    }
}