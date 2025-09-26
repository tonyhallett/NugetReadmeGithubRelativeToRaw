using System.Collections.Generic;

namespace NugetReadmeGithubRelativeToRaw.Rewriter
{
    internal interface IReadmeReplacer
    {
        string Replace(string text, IEnumerable<SourceReplacement> replacements);

    }
}
