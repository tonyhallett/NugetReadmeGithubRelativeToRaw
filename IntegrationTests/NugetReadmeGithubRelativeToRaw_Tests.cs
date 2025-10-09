using NugetBuildTargetsIntegrationTesting;
using NugetReadmeGithubRelativeToRaw.Rewriter;

namespace IntegrationTests
{
    public class NugetReadmeGithubRelativeToRaw_Tests
    {
        private readonly NugetBuildTargetsTestSetup _nugetBuildTargetsTestSetup = new NugetBuildTargetsTestSetup();
        

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
            
            var removeReplaceItems = ReadmeRemoveReplaceItemString.Create(
                "1",
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

            Test(relativeReadme, expectedNuGetReadme, removeReplaceItems);

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

            Test(relativeReadme, expectedNuGetReadme, removeReplaceItems, "", "readme.md", addRelativeFile => addRelativeFile("relative.text", replacement));

        }

        [Test]
        public void Should_Have_RepositoryCommit_When_PublishRepositoryUrl_GitHub()
        {
            PublishRepositoryUrlTest(
                "https://github.com/owner/repo.git",
                (repoRootRelativeImageUrl, commitId) => $"https://raw.githubusercontent.com/owner/repo/{commitId}{repoRootRelativeImageUrl}",
                (repoRootRelativeImageUrl, commitId) => $"https://github.com/owner/repo/blob/{commitId}{repoRootRelativeImageUrl}");
        }

        /*
            "https://gitlab.com/user/repo.git", "main", "https://gitlab.com/user/repo/-/blob/main", "https://gitlab.com/user/repo/-/raw/main" }
        */

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
            Test(repoReadme, expectedNuGetReadme,addRepositoryUrl:false, additionalProperties: additionalProperties, addRelativeFileCallback: addRelativeFile =>
            {
                addRelativeFile("images/image.png", "");
                addRelativeFile("some/page.html", "");

                addRelativeFile(".git/config", gitConfig);
                addRelativeFile(".git/HEAD", headContent);
                addRelativeFile($".git/{headBranchPath}", commitId);
            });
        }


        private void Test(
            string repoReadme,
            string expectedNuGetReadme,
            string removeReplaceItems = "",
            string additionalProperties = "",
            string relativeBaseReadmePath = "readme.md",
            Action<Action<string, string>>? addRelativeFileCallback = null,
            bool addRepositoryUrl = true
            )
        {

            var repositoryUrl = addRepositoryUrl ? "<RepositoryUrl>https://github.com/tonyhallett/arepo.git</RepositoryUrl>" : "";
            DirectoryInfo ? projectDirectory = null;
            var projectWithReadMe = @$"<Project Sdk=""Microsoft.NET.Sdk"">
    <PropertyGroup>
        <TargetFramework>net461</TargetFramework>
        <Authors>TonyHUK</Authors>
        {repositoryUrl}
        <PackageReadmeFile>package-readme.md</PackageReadmeFile>
        <PackageProjectUrl>https://github.com/tonyhallett/arepo</PackageProjectUrl>
        <GeneratePackageOnBuild>True</GeneratePackageOnBuild>
        <BaseReadme>{relativeBaseReadmePath}</BaseReadme>
        <IsPackable>True</IsPackable>
{additionalProperties}
     </PropertyGroup>
     <ItemGroup>
{removeReplaceItems}
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
                    createRelativeFile(relativeBaseReadmePath, repoReadme);
                    addRelativeFileCallback?.Invoke(createRelativeFile);
                });

            if (projectDirectory == null) throw new Exception("Project directory not set");

            var dependentNuGetReadMe = NupkgReadmeReader.Read(projectDirectory!, "package-readme.md");

            Assert.That(dependentNuGetReadMe, Is.EqualTo(expectedNuGetReadme));
        }
    }
}