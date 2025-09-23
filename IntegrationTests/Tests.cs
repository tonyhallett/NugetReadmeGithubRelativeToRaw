using System.IO.Compression;
using System.Reflection;

namespace IntegrationTests
{
    public class Tests
    {
        private NugetTargetPackageTestHelper _nugetTargetPackageTestHelper = new NugetTargetPackageTestHelper();
        private const string NugetReadmeGithubRelativeToRaw = "NugetReadmeGithubRelativeToRaw";

        [SetUp]
        public void Setup()
        {
            _nugetTargetPackageTestHelper = new NugetTargetPackageTestHelper();
        }

        [TearDown]
        public void TearDown()
        {
            _nugetTargetPackageTestHelper.TearDown();
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

            var expectedNugetReadme = @"
Before
![image](https://raw.githubusercontent.com/tonyhallett/arepo/master/images/image.png)
After
";

            var nuPkgPath = GetNuPkgPath();
            _nugetTargetPackageTestHelper.Setup(
                projectWithReadMe,
                nuPkgPath,
                (projectPath) =>
                {
                    projectDirectory = new DirectoryInfo(Path.GetDirectoryName(projectPath)!);
                    CreateRelativeReadmeFile(projectDirectory, relativeReadme);
                });

            if (projectDirectory == null) throw new Exception("Project directory not set");
            
            var dependentNugetReadMe = GetDependentNugetReadMe(projectDirectory!, "package-readme.md");

            Assert.That(dependentNugetReadMe, Is.EqualTo(expectedNugetReadme));
        }

        private static string GetDependentNugetReadMe(DirectoryInfo directoryInfo, string readMeFileName)
        {
            var dependentNugetPath = GetDependentNugetPath(directoryInfo);
            using var zip = ZipFile.OpenRead(dependentNugetPath);

            // nuget always stores readme at the root of the package
            var entry = zip.GetEntry(readMeFileName);

            using var reader = new StreamReader(entry!.Open());
            return reader.ReadToEnd();
        }

        private static string GetDependentNugetPath(DirectoryInfo directoryInfo) => directoryInfo.GetFiles("*.nupkg", SearchOption.AllDirectories).First().FullName;

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