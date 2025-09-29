using System;

namespace NugetReadmeGithubRelativeToRaw.Rewriter
{
    internal static class RewriteTagsOptionsParser
    {
        public static RewriteTagsOptions Parse(string rewriteTagsOptions) => (RewriteTagsOptions)Enum.Parse(typeof(RewriteTagsOptions), rewriteTagsOptions);

    }
}