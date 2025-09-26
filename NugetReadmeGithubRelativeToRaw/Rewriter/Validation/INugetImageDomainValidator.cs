namespace NugetReadmeGithubRelativeToRaw.Rewriter.Validation
{
    internal interface INugetImageDomainValidator
    {
        bool IsTrustedImageDomain(string uriString);
    }
}