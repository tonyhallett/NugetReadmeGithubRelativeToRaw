using System.Collections.Generic;

namespace NugetReadmeGithubRelativeToRaw
{
    internal interface IReadmeReplacer
    {
        string Replace(string text, IEnumerable<SourceReplacement> replacements);

    }
}
