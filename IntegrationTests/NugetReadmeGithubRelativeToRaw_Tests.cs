using NugetBuildTargetsIntegrationTesting;
using NugetReadmeGithubRelativeToRaw.Rewriter;

namespace IntegrationTests
{
    public class NugetReadmeGithubRelativeToRaw_Tests
    {
        private readonly NugetBuildTargetsTestSetup _nugetBuildTargetsTestSetup = new NugetBuildTargetsTestSetup();

        private record RepoReadme(string Readme, string RelativePath = "readme.md", bool AddProjectElement = true, bool AddReadme = true);

        private const string DefaultPackageReadmeFileElementContents = "package-readme.md";

        private record GeneratedReadme(
            string Expected,
            string PackageReadMeFileElementContents = DefaultPackageReadmeFileElementContents,
            string ZipEntryName = "package-readme.md",
            string ExpectedOutputPath = "obj\\Release\\net461\\ReadmeRewrite\\package-readme.md")
        { 
        
            public static GeneratedReadme Simple(string expected) => new GeneratedReadme(expected);
            
            public static GeneratedReadme PackagePath(string expected, string packageReadMeFileElementContents)
                => new GeneratedReadme(expected, packageReadMeFileElementContents, packageReadMeFileElementContents.Replace('\\', '/'));

            public static GeneratedReadme OutputPath(string expected, string expectedOutputPath)
                => new GeneratedReadme(expected, ExpectedOutputPath: expectedOutputPath);
        }

        private record ProjectFileAdditional(string Properties, string RemoveReplaceItems)
        {
            public static ProjectFileAdditional None { get; } = new ProjectFileAdditional("", "");

            public static ProjectFileAdditional PropertiesOnly(string properties)
            {
                return new ProjectFileAdditional(properties, "");
            }

            public static ProjectFileAdditional RemoveReplaceItemsOnly(string removeReplaceItems)
            {
                return new ProjectFileAdditional("", removeReplaceItems);
            }
        }


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

            Test(
                new RepoReadme(relativeReadme, "readmedir/readme.md"), 
                GeneratedReadme.Simple(expectedNuGetReadme),
                null,
                addRelativeFile => addRelativeFile("images/image.png",""));
        }

        [Test]
        public void Should_Have_Correct_Replaced_To_End_Inline_Readme_In_Generated_NuPkg()
        {
            var replace = "# Replace";
            var replacement = "Nuget only";
            
            var removeReplaceItems = ReadmeRemoveReplaceItemString.Create(
                "1",
                [
                    ReadmeRemoveReplaceItemString.StartElement(replace),
                    ReadmeRemoveReplaceItemString.CommentOrRegexElement(CommentOrRegex.Regex),
                    ReadmeRemoveReplaceItemString.ReplacementTextElement(replacement)
                ]);

            var repoReadme = @$"
Before
{replace}
This will be replaced
";

            var expectedNuGetReadme = @$"
Before
{replacement}";

            Test(new RepoReadme(repoReadme), GeneratedReadme.Simple(expectedNuGetReadme), ProjectFileAdditional.RemoveReplaceItemsOnly(removeReplaceItems));

        }

        [Test]
        public void Should_Have_Correct_Replaced_To_End_From_File_Readme_In_Generated_NuPkg()
        {
            var replace = "# Replace";
            var replacement = "Nuget only file replace";

            var removeReplaceItems = ReadmeRemoveReplaceItemString.Create("replace.txt",
                [
                    ReadmeRemoveReplaceItemString.StartElement(replace),
                    ReadmeRemoveReplaceItemString.CommentOrRegexElement(CommentOrRegex.Regex),
                    ReadmeRemoveReplaceItemString.ReplacementTextElement(replacement)
                ]);
            var relativeReadme = @$"
Before
{replace}
This will be replaced
";

            var expectedNuGetReadme = @$"
Before
{replacement}";

            Test(new RepoReadme(relativeReadme), GeneratedReadme.Simple(expectedNuGetReadme), ProjectFileAdditional.RemoveReplaceItemsOnly(removeReplaceItems), addRelativeFile => addRelativeFile("relative.text", replacement));
        }

        [Test]
        public void Should_Have_RepositoryCommit_When_PublishRepositoryUrl_GitHub()
        {
            PublishRepositoryUrlTest(
                "https://github.com/owner/repo.git",
                (repoRootRelativeImageUrl, commitId) => $"https://raw.githubusercontent.com/owner/repo/{commitId}{repoRootRelativeImageUrl}",
                (repoRootRelativeImageUrl, commitId) => $"https://github.com/owner/repo/blob/{commitId}{repoRootRelativeImageUrl}");
        }

        [Test]
        public void Should_Have_RepositoryCommit_When_PublishRepositoryUrl_GitLab()
        {
            PublishRepositoryUrlTest(
                "https://gitlab.com/user/repo.git",
                (repoRootRelativeImageUrl, commitId) => $"https://gitlab.com/user/repo/-/raw/{commitId}{repoRootRelativeImageUrl}",
                (repoRootRelativeImageUrl, commitId) => $"https://gitlab.com/user/repo/-/blob/{commitId}{repoRootRelativeImageUrl}");
        }

        private void PublishRepositoryUrlTest(
            string remoteUrl, 
            Func<string,string,string> getExpectedAbsoluteImageUrl, 
            Func<string,string,string> getExpectedAbsoluteLinkUrl)
        {

            var additionalProperties = "<PublishRepositoryUrl>True</PublishRepositoryUrl>";

            var commitId = "f5eb304528a94c667be2ab0f921b3995746c7ce8";
            var gitConfig = $@"
[core]
	repositoryformatversion = 0
[remote ""origin""]
	url = {remoteUrl}
";
            var headBranchPath = "refs/heads/myBranch";
            var headContent = $"ref: {headBranchPath}";

            // relative to repo root
            var relativeImageUrl = "/images/image.png";
            var relativeLinkUrl = "/some/page.html";
            var repoReadme = $@"
![image]({relativeImageUrl})

[link]({relativeLinkUrl})";

            var expectedNuGetReadme = $@"
![image]({getExpectedAbsoluteImageUrl(relativeImageUrl,commitId)})

[link]({getExpectedAbsoluteLinkUrl(relativeLinkUrl, commitId)})";
            Test(
                new RepoReadme(repoReadme), 
                GeneratedReadme.Simple(expectedNuGetReadme),
                addRepositoryUrl:false, 
                projectFileAdditional: ProjectFileAdditional.PropertiesOnly(additionalProperties), 
                addRelativeFileCallback: addRelativeFile =>
                {
                    addRelativeFile("images/image.png", "");
                    addRelativeFile("some/page.html", "");

                    addRelativeFile(".git/config", gitConfig);
                    addRelativeFile(".git/HEAD", headContent);
                    addRelativeFile($".git/{headBranchPath}", commitId);
                });
        }

        [Test]
        public void Should_Permit_Nested_Package_Path()
        {
            var repoReadme = new RepoReadme("untouched");
            var generatedReadme = GeneratedReadme.PackagePath("untouched", "docs\\package-readme.md");
            Test(repoReadme, generatedReadme);
        }

        [Test]
        public void Should_Generate_To_ReadmeRewrite_In_Obj()
        {
            Different_Output_Paths_Test(null, Path.Combine("obj", "Release", "net461", "ReadmeRewrite", DefaultPackageReadmeFileElementContents));
        }

        [Test]
        public void Should_Generate_To_MSBuild_GeneratedReadmeDirectory_When_Absolute()
        {
            var tmpOutputDirectoryPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            try
            {
                Different_Output_Paths_Test(tmpOutputDirectoryPath, Path.Combine(tmpOutputDirectoryPath, DefaultPackageReadmeFileElementContents));
            }
            finally
            {
                if (File.Exists(tmpOutputDirectoryPath))
                {
                    File.Delete(tmpOutputDirectoryPath);
                }
            }
        }

        [Test]
        public void Should_Generate_Relative_To_Project_Directory_When_GeneratedReadmeDirectory_Is_Relative()
        {
            var relativeProjectDir = "projectsubdir";
            Different_Output_Paths_Test( relativeProjectDir, Path.Combine(relativeProjectDir, DefaultPackageReadmeFileElementContents));
        }

        private void Different_Output_Paths_Test(string? generatedReadmeDirectory, string expectedOutputPath)
        {
            var repoReadme = new RepoReadme("untouched");
            var generatedReadme = GeneratedReadme.OutputPath("untouched", expectedOutputPath);
            var projectFileAdditional = generatedReadmeDirectory == null ? null : ProjectFileAdditional.PropertiesOnly($"<GeneratedReadmeDirectory>{generatedReadmeDirectory}</GeneratedReadmeDirectory>");
            Test(repoReadme, generatedReadme, projectFileAdditional);
        }

        private void Test(
            RepoReadme repoReadme,
            GeneratedReadme generatedReadme,
            ProjectFileAdditional? projectFileAdditional = null,
            Action<Action<string, string>>? addRelativeFileCallback = null,
            bool addRepositoryUrl = true
            )
        {
            projectFileAdditional = projectFileAdditional ?? ProjectFileAdditional.None;
            var baseReadmeElementOrEmptyString = repoReadme.AddProjectElement ? $"<BaseReadme>{repoReadme.RelativePath}</BaseReadme>" : "";
            var repositoryUrlElementOrEmptyString = addRepositoryUrl ? "<RepositoryUrl>https://github.com/tonyhallett/arepo.git</RepositoryUrl>" : "";
            DirectoryInfo ? projectDirectory = null;
            var projectWithReadMe = @$"<Project Sdk=""Microsoft.NET.Sdk"">
    <PropertyGroup>
        <TargetFramework>net461</TargetFramework>
        <Authors>TonyHUK</Authors>
        {repositoryUrlElementOrEmptyString}
        <PackageReadmeFile>{generatedReadme.PackageReadMeFileElementContents}</PackageReadmeFile>
        <PackageProjectUrl>https://github.com/tonyhallett/arepo</PackageProjectUrl>
        <GeneratePackageOnBuild>True</GeneratePackageOnBuild>
        {baseReadmeElementOrEmptyString}
        <IsPackable>True</IsPackable>
{projectFileAdditional.Properties}
     </PropertyGroup>
     <ItemGroup>
{projectFileAdditional.RemoveReplaceItems}
     </ItemGroup>
</Project>
";

            var nuPkgPath = NupkgProvider.GetNuPkgPath();
            _nugetBuildTargetsTestSetup.Setup(
                projectWithReadMe,
                nuPkgPath,
                (projectPath) =>
                {
                    projectDirectory = new DirectoryInfo(Path.GetDirectoryName(projectPath)!);
                    Action<string, string> createRelativeFile = (relativeFile, contents) => FileHelper.WriteAllTextEnsureDirectory(Path.Combine(projectDirectory.FullName, relativeFile), contents);
                    if (repoReadme.AddReadme)
                    {
                        createRelativeFile(repoReadme.RelativePath, repoReadme.Readme);
                    }
                    addRelativeFileCallback?.Invoke(createRelativeFile);
                });

            if (projectDirectory == null) throw new Exception("Project directory not set");

            var dependentNuGetReadMe = NupkgReadmeReader.Read(projectDirectory, generatedReadme.ZipEntryName);

            Assert.That(dependentNuGetReadMe, Is.EqualTo(generatedReadme.Expected));

            string expectedOutputPath;
            if (Path.IsPathRooted(generatedReadme.ExpectedOutputPath))
            {
                expectedOutputPath = generatedReadme.ExpectedOutputPath;
            }
            else
            {
                expectedOutputPath = Path.Combine(projectDirectory.FullName, generatedReadme.ExpectedOutputPath);
            }

            Assert.That(File.Exists(expectedOutputPath), Is.True, $"Expected generated path {expectedOutputPath} to exist");
        }
    }
}