using System.Diagnostics.CodeAnalysis;

namespace NugetReadmeGithubRelativeToRaw
{
    [ExcludeFromCodeCoverage]
    internal class ReadmeRelativeFileExistsFactory : IReadmeRelativeFileExistsFactory
    {
        public IReadmeRelativeFileExists Create(string projectDirectoryPath,string readmeRelativePath, IIOHelper ioHelper)
        {
            return new ReadmeRelativeFileExists(projectDirectoryPath, readmeRelativePath, ioHelper);
        }
    }
}
