using System.Linq;

namespace NugetReadmeGithubRelativeToRaw.Rewriter
{
    internal static class MarkdownElementsProcessResultExtensions
    {
        public static bool HasErrors(this IMarkdownElementsProcessResult markdownElementsProcessResult)
            => markdownElementsProcessResult.MissingReadmeAssets.Any() || markdownElementsProcessResult.UnsupportedImageDomains.Any();
    }
}
