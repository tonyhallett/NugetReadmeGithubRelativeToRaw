namespace NugetReadmeGithubRelativeToRaw.Rewriter
{
    internal interface IRemoveReplacer { 
    
        string RemoveReplace(string text, RemoveReplaceSettings removeReplaceSettings);
    }
}
