using System.IO;

namespace NugetReadmeGithubRelativeToRaw
{
    internal class ReadmeRelativeFileExists : IReadmeRelativeFileExists {
        private readonly string _projectDirectoryPath;
        private readonly string _readmeRelativePath;
        private readonly IIOHelper _ioHelper;

        public ReadmeRelativeFileExists(string projectDirectoryPath, string readmeRelativePath, IIOHelper ioHelper)
        {
            readmeRelativePath = NormalizeDirectorySeparators(readmeRelativePath);
            _projectDirectoryPath = projectDirectoryPath;
            _readmeRelativePath = readmeRelativePath;
            _ioHelper = ioHelper;
        }

        public bool Exists(string relativePath)
        {
            return _ioHelper.FileExists(GetPath(relativePath));
        }

        private string NormalizeDirectorySeparators(string path)
        {
            return path
                .Replace('/', Path.DirectorySeparatorChar)
                .Replace('\\', Path.DirectorySeparatorChar);
        }

        private string GetPath(string relativePath)
        {
            relativePath = NormalizeDirectorySeparators(relativePath);

            // repo relative
            if (relativePath.StartsWith(Path.DirectorySeparatorChar.ToString()))
            {
                return Path.Combine(_projectDirectoryPath, relativePath.TrimStart(Path.DirectorySeparatorChar));
            }

            string readmeFullPath = Path.Combine(_projectDirectoryPath, _readmeRelativePath);
            string readmeDirectory = Path.GetDirectoryName(readmeFullPath)!;
            return Path.Combine(readmeDirectory, relativePath);
        }
    }
}
