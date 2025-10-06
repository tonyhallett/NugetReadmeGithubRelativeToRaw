namespace NugetReadmeGithubRelativeToRaw
{
    internal interface IReadmeRelativeFileExistsFactory
    {
        IReadmeRelativeFileExists Create(string projectDirectoryPath, string readmeRelativePath, IIOHelper ioHelper);
    }
}
