using System.IO;

namespace NugetReadmeGithubRelativeToRaw
{
    internal class ReadmeRelativeFileExists : IReadmeRelativeFileExists {
        public string ProjectDirectoryPath { get; }
        public string ReadmeRelativePath { get; }

        public ReadmeRelativeFileExists(string projectDirectoryPath, string readmeRelativePath)
        {
            readmeRelativePath = NormalizeDirectorySeparators(readmeRelativePath);
            ProjectDirectoryPath = projectDirectoryPath;
            ReadmeRelativePath = readmeRelativePath;
        }

        public bool Exists(string relativePath)
        {
            return File.Exists(GetPath(relativePath));
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
                return Path.Combine(ProjectDirectoryPath, relativePath.TrimStart(Path.DirectorySeparatorChar));
            }

            string readmeFullPath = Path.Combine(ProjectDirectoryPath, ReadmeRelativePath);
            string readmeDirectory = Path.GetDirectoryName(readmeFullPath)!;
            return Path.Combine(readmeDirectory, relativePath);
        }
    }
}
