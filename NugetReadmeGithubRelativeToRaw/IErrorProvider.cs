namespace NugetReadmeGithubRelativeToRaw
{
    internal interface IErrorProvider
    {
        string ProvideRequiredMetadataError(string metadataName,string itemName, string itemSpec);
        
        string ProvideUnsupportedCommentOrRegex(string metadataName, string itemName, string itemSpec, string supportedValues);
    }
}
