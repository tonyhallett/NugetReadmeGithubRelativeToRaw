namespace NugetReadmeGithubRelativeToRaw
{
    internal class ReadmeRelativeFileExistsFactory : IReadmeRelativeFileExistsFactory
    {
        public IReadmeRelativeFileExists Create(string projectDirectoryPath,string readmeRelativePath, IIOHelper ioHelper)
        {
            return new ReadmeRelativeFileExists(projectDirectoryPath, readmeRelativePath, ioHelper);
        }
    }
}
