using System.Reflection;

namespace IntegrationTests
{
    internal static class NupkgProvider
    {
        private const string NugetReadmeGithubRelativeToRaw = "NugetReadmeGithubRelativeToRaw";

        public static string GetNuPkgPath()
        {
            var projectDirectory = GetProjectDirectory();
#if DEBUG
            var debugOrRelease = "Debug";
#else
            var debugOrRelease = "Release";
#endif
            var debugOrReleaseDirectory = projectDirectory.GetDescendantDirectory("bin", debugOrRelease);
            return debugOrReleaseDirectory.GetFiles("*.nupkg", SearchOption.AllDirectories).First().FullName;

        }

        private static DirectoryInfo GetSolutionDirectory()
        {
            var directory = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory;
            while (directory != null && directory.Name != NugetReadmeGithubRelativeToRaw)
            {
                directory = directory.Parent;
            }
            if (directory == null) throw new Exception("Could not find solution directory");
            return directory;
        }

        private static DirectoryInfo GetProjectDirectory()
        {
            var solutionDirectory = GetSolutionDirectory();
            return new DirectoryInfo(Path.Combine(solutionDirectory.FullName, NugetReadmeGithubRelativeToRaw));
        }

    }
}