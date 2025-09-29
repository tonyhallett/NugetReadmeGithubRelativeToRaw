using System.IO.Compression;
using System.Reflection;
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
            DirectoryInfo? projectDirectory = null;
            var projectWithReadMe = @"<Project Sdk=""Microsoft.NET.Sdk"">
    <PropertyGroup>
        <TargetFramework>net461</TargetFramework>
        <Authors>TonyHUK</Authors>
        <RepositoryUrl>https://github.com/tonyhallett/arepo.git</RepositoryUrl>
        <PackageReadmeFile>package-readme.md</PackageReadmeFile>
        <PackageProjectUrl>https://github.com/tonyhallett/arepo</PackageProjectUrl>
        <GeneratePackageOnBuild>True</GeneratePackageOnBuild>
        <IsPackable>True</IsPackable>
     </PropertyGroup>
</Project>
"; // todo

            var relativeReadme = @"
Before
![image](images/image.png)
After
";

            var expectedNuGetReadme = @"
Before
![image](https://raw.githubusercontent.com/tonyhallett/arepo/master/images/image.png)
After
";
            
            var nuPkgPath = GetNuPkgPath();
            _nugetBuildTargetsTestSetup.Setup(
                projectWithReadMe,
                nuPkgPath,
                (projectPath) =>
                {
                    projectDirectory = new DirectoryInfo(Path.GetDirectoryName(projectPath)!);
                    CreateRelativeReadmeFile(projectDirectory, relativeReadme);
                });

            if (projectDirectory == null) throw new Exception("Project directory not set");

            var dependentNuGetReadMe = GetDependentNuGetReadMe(projectDirectory!, "package-readme.md");

            Assert.That(dependentNuGetReadMe, Is.EqualTo(expectedNuGetReadme));
            
        }

        [Test]
        public void Should_Have_Correct_Replaced_To_End_Inline_Readme_In_Generated_NuPkg()
        {
            var replace = "# Replace";
            var replacement = "Nuget only";

            DirectoryInfo? projectDirectory = null;
            var projectWithReadMe = @$"<Project Sdk=""Microsoft.NET.Sdk"">
    <PropertyGroup>
        <TargetFramework>net461</TargetFramework>
        <Authors>TonyHUK</Authors>
        <RepositoryUrl>https://github.com/tonyhallett/arepo.git</RepositoryUrl>
        <PackageReadmeFile>package-readme.md</PackageReadmeFile>
        <PackageProjectUrl>https://github.com/tonyhallett/arepo</PackageProjectUrl>
        <GeneratePackageOnBuild>True</GeneratePackageOnBuild>
        <IsPackable>True</IsPackable>
     </PropertyGroup>
     <ItemGroup>
        <RemoveReplaceItem Include=""1"">
            {CreateMetadataElement(RemoveReplaceSettingsMetadataNames.Start, replace)}
            {CreateMetadataElement(RemoveReplaceSettingsMetadataNames.CommentOrRegex,nameof(CommentOrRegex.Regex))}
            {CreateMetadataElement(RemoveReplaceSettingsMetadataNames.ReplacementText, replacement)}
            <ReplacementText>Nuget only</ReplacementText>
        </RemoveReplaceItem>
     </ItemGroup>
</Project>
"; // todo

            var relativeReadme = @$"
Before
{replace}
This will be replaced
";

            var expectedNuGetReadme = @$"
Before
{replacement}
";

            var nuPkgPath = GetNuPkgPath();
            _nugetBuildTargetsTestSetup.Setup(
                projectWithReadMe,
                nuPkgPath,
                (projectPath) =>
                {
                    projectDirectory = new DirectoryInfo(Path.GetDirectoryName(projectPath)!);
                    CreateRelativeReadmeFile(projectDirectory, relativeReadme);
                });

            if (projectDirectory == null) throw new Exception("Project directory not set");

            var dependentNuGetReadMe = GetDependentNuGetReadMe(projectDirectory!, "package-readme.md");

            Assert.That(dependentNuGetReadMe, Is.EqualTo(expectedNuGetReadme));

        }

        private string CreateMetadataElement(string metadataName, string contents)
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

        private static void CreateRelativeReadmeFile(DirectoryInfo projectDirectory, string readMe)
            => File.WriteAllText(Path.Combine(projectDirectory.FullName, "readme.md"), readMe);

        private static string GetNuPkgPath() => GetProjectDirectory().GetFiles("*.nupkg", SearchOption.AllDirectories).First().FullName;

        private static DirectoryInfo GetSolutionDirectory()
        {
            var directory = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory;
            while (directory != null && directory.Name != NugetReadmeGithubRelativeToRaw)
            {
                directory = directory.Parent;
            }
            if(directory == null) throw new Exception("Could not find solution directory");
            return directory;
        }

        private static DirectoryInfo GetProjectDirectory()
        {
            var solutionDirectory = GetSolutionDirectory();
            return new DirectoryInfo(Path.Combine(solutionDirectory.FullName, NugetReadmeGithubRelativeToRaw));
        }
    }
}