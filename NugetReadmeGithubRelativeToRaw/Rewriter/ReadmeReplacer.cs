using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NugetReadmeGithubRelativeToRaw.Rewriter
{
    internal class ReadmeReplacer : IReadmeReplacer
    {
        public string Replace(string text, IEnumerable<SourceReplacement> replacements)
        {
            var sb = new StringBuilder(text);

            var ordered = replacements.OrderByDescending(r => r.Start).ToList();
            foreach (var replacement in ordered)
            {
                sb.Remove(replacement.Start, replacement.End - replacement.Start + 1);
                sb.Insert(replacement.Start, replacement.Replacement);
            }

            return sb.ToString();
        }
    }
}
