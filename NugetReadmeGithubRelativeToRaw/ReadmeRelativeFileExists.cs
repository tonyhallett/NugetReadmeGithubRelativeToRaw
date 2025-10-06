using System.IO;

namespace NugetReadmeGithubRelativeToRaw
{
    internal class ReadmeRelativeFileExists : IReadmeRelativeFileExists {
        private readonly string _projectDirectoryPath;
        private readonly string _readmeRelativePath;
        private readonly IIOHelper _ioHelper;

        public ReadmeRelativeFileExists(string projectDirectoryPath, string readmeRelativePath, IIOHelper ioHelper)
        {
            _projectDirectoryPath = projectDirectoryPath;
            _readmeRelativePath = readmeRelativePath;
            _ioHelper = ioHelper;
        }

        public bool Exists(string relativePath)
        {
            return _ioHelper.FileExists(GetPath(relativePath));
        }

        private string GetPath(string relativePath)
        {
            // repo relative
            if (relativePath.StartsWith("/"))
            {
                return Path.Combine(_projectDirectoryPath, relativePath);
            }

            var readmeRelativePath = relativePath;
            if (!readmeRelativePath.StartsWith("/"))
            {
                readmeRelativePath = "/" + readmeRelativePath;
            }

            return Path.Combine(_projectDirectoryPath, readmeRelativePath);
        }
    }
}
