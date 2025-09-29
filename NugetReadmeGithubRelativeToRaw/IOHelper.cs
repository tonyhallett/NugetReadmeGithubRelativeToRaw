using System.IO;

namespace NugetReadmeGithubRelativeToRaw
{
    internal class IOHelper : IIOHelper
    {
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

        public void WriteAllText(string path, string contents)
        {
            File.WriteAllText(path, contents);
        }
    }
}
