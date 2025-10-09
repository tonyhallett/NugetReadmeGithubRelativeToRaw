using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace NugetReadmeGithubRelativeToRaw
{
    [ExcludeFromCodeCoverage]
    internal class IOHelper : IIOHelper
    {
        public static IOHelper Instance { get; } = new IOHelper();

        public string CombinePaths(string path1, string path2)
        {
            return Path.Combine(path1, path2);
        }

        public bool FileExists(string filePath)
        {
            return File.Exists(filePath);
        }

        public string ReadAllText(string readmePath)
        {
            return File.ReadAllText(readmePath);
        }

        public void WriteAllTextEnsureDirectory(string path, string contents)
        {
            var directory = Path.GetDirectoryName(path);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory!);
            }

            File.WriteAllText(path, contents);
        }
    }
}
