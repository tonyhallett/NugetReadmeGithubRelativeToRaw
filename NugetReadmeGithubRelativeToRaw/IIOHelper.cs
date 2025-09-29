namespace NugetReadmeGithubRelativeToRaw
{
    internal interface IIOHelper
    {
        bool FileExists(string filePath);
        string CombinePaths(string path1, string path2);
        void WriteAllText(string outputReadme, string rewrittenReadme);
        string ReadAllText(string readmePath);
    }
}
