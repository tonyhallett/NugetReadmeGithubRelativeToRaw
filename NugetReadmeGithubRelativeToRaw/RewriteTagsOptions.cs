using System;

namespace NugetReadmeGithubRelativeToRaw
{
    [Flags]
    internal enum RewriteTagsOptions
    {
        None = 0,
        RewriteImgTagsForSupportedDomains = 1,
        RewriteATags = 2,
        RewriteBrTags = 4,
        All = RewriteImgTagsForSupportedDomains | RewriteATags | RewriteBrTags
    }
}
