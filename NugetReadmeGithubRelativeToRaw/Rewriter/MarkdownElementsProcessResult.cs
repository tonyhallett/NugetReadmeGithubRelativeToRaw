using System;
using System.Collections.Generic;
using Markdig.Syntax;

namespace NugetReadmeGithubRelativeToRaw.Rewriter
{
    internal class MarkdownElementsProcessResult : IMarkdownElementsProcessResult
    {
        private readonly List<SourceReplacement> _sourceReplacements = new List<SourceReplacement>();
        private readonly HashSet<string> unsupportedImageDomains = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> missingReadmeAssets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public IEnumerable<string> UnsupportedImageDomains => unsupportedImageDomains;

        public IEnumerable<string> MissingReadmeAssets => missingReadmeAssets;

        public IEnumerable<SourceReplacement> SourceReplacements => _sourceReplacements;

        public void AddSourceReplacement(SourceSpan sourceSpan, string replacement)
            => _sourceReplacements.Add(new SourceReplacement(sourceSpan, replacement));

        public void AddUnsupportedImageDomain(string domain) => unsupportedImageDomains.Add(domain);

        internal void AddMissingReadmeAsset(string url) => missingReadmeAssets.Add(url);
    }
}
