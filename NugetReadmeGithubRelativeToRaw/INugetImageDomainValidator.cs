namespace NugetReadmeGithubRelativeToRaw
{
    internal interface INugetImageDomainValidator
    {
        bool IsTrustedImageDomain(string uriString);
    }
}