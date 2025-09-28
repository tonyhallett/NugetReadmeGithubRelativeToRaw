namespace NugetReadmeGithubRelativeToRaw.Rewriter.Validation
{
    internal interface  INuGetTrustedImageDomains
    {
        bool IsImageDomainTrusted(string imageDomain);
    }
}
