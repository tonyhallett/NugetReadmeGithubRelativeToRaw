using NugetReadmeGithubRelativeToRaw;

namespace UnitTests
{
    internal sealed class DummyReadmeRelativeFileExists : IReadmeRelativeFileExists
    {
        public bool FileExists { get; set; } = true;

        public string? RelativePath { get; private set; }
        
        public bool Exists(string relativePath)
        {
            RelativePath = relativePath;
            return FileExists;
        }
    }
}