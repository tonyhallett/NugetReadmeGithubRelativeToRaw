namespace NugetReadmeGithubRelativeToRaw.Rewriter.Validation
{
    internal interface INuGetImageDomainValidator
    {
        bool IsTrustedImageDomain(string uriString);
    }
}