using System.IO.Compression;
using System.Reflection;
using System.Text;
using NugetBuildTargetsIntegrationTesting;
using NugetReadmeGithubRelativeToRaw;
using NugetReadmeGithubRelativeToRaw.Rewriter;

namespace IntegrationTests
{
    public class NugetReadmeGithubRelativeToRaw_Tests
    {
        private readonly NugetBuildTargetsTestSetup _nugetBuildTargetsTestSetup = new NugetBuildTargetsTestSetup();
        private const string NugetReadmeGithubRelativeToRaw = "NugetReadmeGithubRelativeToRaw";

        [OneTimeTearDown]
        public void TearDown()
        {
            _nugetBuildTargetsTestSetup.TearDown();
        }

        [Test]
        public void Should_Have_Correct_ReadMe_In_Generated_NuPkg()
        {
            // relative to repo root
            var relativeReadme = @"
Before
![image](/images/image.png)
After
";

            var expectedNuGetReadme = @"
Before
![image](https://raw.githubusercontent.com/tonyhallett/arepo/master/images/image.png)
After
";

            Test(relativeReadme, expectedNuGetReadme,"","","readmedir/readme.md",addRelativeFile => addRelativeFile("images/image.png",""));
        }

        [Test]
        public void Should_Have_Correct_Replaced_To_End_Inline_Readme_In_Generated_NuPkg()
        {
            var replace = "# Replace";
            var replacement = "Nuget only";
            
            var removeReplaceItems = CreateReadmeRemoveReplaceItem(
                "1",
                [
                    CreateMetadataElement(nameof(RemoveReplaceMetadata.Start), replace),
                    CreateMetadataElement(nameof(RemoveReplaceMetadata.CommentOrRegex), nameof(CommentOrRegex.Regex)),
                    CreateMetadataElement(nameof(RemoveReplaceMetadata.ReplacementText), replacement)
                ]);

            var relativeReadme = @$"
Before
{replace}
This will be replaced
";

            var expectedNuGetReadme = @$"
Before
{replacement}";

            Test(relativeReadme, expectedNuGetReadme, removeReplaceItems);

        }

        private static string CreateReadmeRemoveReplaceItem(string include, IEnumerable<string> metadata)
        {
            var sb = new StringBuilder();
            sb.AppendLine(@$"<{MsBuildPropertyItemNames.ReadmeRemoveReplace} Include=""{include}"">");
            foreach (var meta in metadata)
            {
                sb.AppendLine(meta);
            }
            sb.AppendLine($@"</{MsBuildPropertyItemNames.ReadmeRemoveReplace}>");
            return sb.ToString();
        }

        [Test]
        public void Should_Have_Correct_Replaced_To_End_From_File_Readme_In_Generated_NuPkg()
        {
            var replace = "# Replace";
            var replacement = "Nuget only file replace";

            var removeReplaceItems = CreateReadmeRemoveReplaceItem("replace.txt",
                [
                    CreateMetadataElement(nameof(RemoveReplaceMetadata.Start), replace),
                    CreateMetadataElement(nameof(RemoveReplaceMetadata.CommentOrRegex), nameof(CommentOrRegex.Regex)),
                    CreateMetadataElement(nameof(RemoveReplaceMetadata.ReplacementText), replacement)
                ]);
            var relativeReadme = @$"
Before
{replace}
This will be replaced
";

            var expectedNuGetReadme = @$"
Before
{replacement}";

            Test(relativeReadme, expectedNuGetReadme, removeReplaceItems, "", "readme.md", addRelativeFile => addRelativeFile("relative.text", replacement));

        }

        private void Test(
            string relativeReadme,
            string expectedNuGetReadme,
            string removeReplaceItems = "",
            string additionalProperties = "",
            string relativeReadmePath = "readme.md",
            Action<Action<string, string>>? addRelativeFileCallback = null)
        {
            DirectoryInfo? projectDirectory = null;
            var projectWithReadMe = @$"<Project Sdk=""Microsoft.NET.Sdk"">
    <PropertyGroup>
        <TargetFramework>net461</TargetFramework>
        <Authors>TonyHUK</Authors>
        <RepositoryUrl>https://github.com/tonyhallett/arepo.git</RepositoryUrl>
        <PackageReadmeFile>package-readme.md</PackageReadmeFile>
        <PackageProjectUrl>https://github.com/tonyhallett/arepo</PackageProjectUrl>
        <GeneratePackageOnBuild>True</GeneratePackageOnBuild>
        <BaseReadme>{relativeReadmePath}</BaseReadme>
        <IsPackable>True</IsPackable>
{additionalProperties}
     </PropertyGroup>
     <ItemGroup>
{removeReplaceItems}
     </ItemGroup>
</Project>
"; // todo

            var nuPkgPath = GetNuPkgPath();
            _nugetBuildTargetsTestSetup.Setup(
                projectWithReadMe,
                nuPkgPath,
                (projectPath) =>
                {
                    projectDirectory = new DirectoryInfo(Path.GetDirectoryName(projectPath)!);
                    Action<string, string> createRelativeFile = (relativeFile, contents) => SafeFileWriteAllText(Path.Combine(projectDirectory.FullName, relativeFile), contents);
                    createRelativeFile(relativeReadmePath, relativeReadme);
                    addRelativeFileCallback?.Invoke(createRelativeFile);
                });

            if (projectDirectory == null) throw new Exception("Project directory not set");

            var dependentNuGetReadMe = GetDependentNuGetReadMe(projectDirectory!, "package-readme.md");

            Assert.That(dependentNuGetReadMe, Is.EqualTo(expectedNuGetReadme));
        }

        private static string CreateMetadataElement(string metadataName, string contents)
            => $"<{metadataName}>{contents}</{metadataName}>";

        private static string GetDependentNuGetReadMe(DirectoryInfo directoryInfo, string readMeFileName)
        {
            var dependentNuGetPath = GetDependentNuGetPath(directoryInfo);
            using var zip = ZipFile.OpenRead(dependentNuGetPath);

            // nuget always stores readme at the root of the package
            var entry = zip.GetEntry(readMeFileName);

            using var reader = new StreamReader(entry!.Open());
            return reader.ReadToEnd();
        }

        private static string GetDependentNuGetPath(DirectoryInfo directoryInfo) => directoryInfo.GetFiles("*.nupkg", SearchOption.AllDirectories).First().FullName;

        private static string GetNuPkgPath()
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

        private static void SafeFileWriteAllText(string path, string contents)
        {
            var directory = Path.GetDirectoryName(path);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory!);
            }
            File.WriteAllText(path, contents);
        }
    }

    public static class DirectoryInfoExtensions
    {
        public static DirectoryInfo GetDescendantDirectory(this DirectoryInfo directory, params string[] paths)
        {
            foreach (var path in paths)
            {
                directory = new DirectoryInfo(Path.Combine(directory.FullName, path));
            }

            return directory;
        }
    }
}