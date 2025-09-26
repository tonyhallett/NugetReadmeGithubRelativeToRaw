namespace NugetReadmeGithubRelativeToRaw.Rewriter.Validation
{
    internal interface  INugetTrustedImageDomains
    {
        bool IsImageDomainTrusted(string imageDomain);
    }
}
