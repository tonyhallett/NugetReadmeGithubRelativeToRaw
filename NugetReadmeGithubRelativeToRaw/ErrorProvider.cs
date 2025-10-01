namespace NugetReadmeGithubRelativeToRaw
{
    internal class ErrorProvider : IErrorProvider
    {
        public static ErrorProvider Instance { get; } = new ErrorProvider();
        public string ProvideRequiredMetadataError(string metadataName, string itemName, string itemSpec)
            => $"Metadata, {metadataName}, is required on item {itemName} '{itemSpec}'.";

        public string ProvideUnsupportedCommentOrRegex(string metadataName, string itemName, string itemSpec, string supportedValues) 
            => $"Unsupported {metadataName} metadata on item {itemName} '{itemSpec}'. Supported values {supportedValues}";
    }
}
