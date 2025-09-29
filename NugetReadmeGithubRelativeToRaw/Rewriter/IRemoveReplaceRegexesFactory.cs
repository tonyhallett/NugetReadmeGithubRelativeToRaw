namespace NugetReadmeGithubRelativeToRaw.Rewriter
{
    internal interface IRemoveReplaceRegexesFactory
    {
        IRemoveReplaceRegexes Create(RemoveReplaceSettings removeReplaceSettings);
    }
}
