using System.Xml.Linq;

namespace IntegrationTests
{
    public class NugetTargetPackageTestHelper
    {
        private readonly INugetHelper _nugetHelper;
        private readonly IProjectBuilder _projectBuilder;
        private readonly IIOUtilities _ioUtilities;
        private string? _dependentProjectDirectory;

        public NugetTargetPackageTestHelper() : 
            this(new NugetHelper( IOUtilities.Instance, new NugetAddCommand(), new MsBuildProjectHelper()), new DotNetProjectBuilder(), IOUtilities.Instance)
        {
        }

        internal NugetTargetPackageTestHelper(
            INugetHelper nugetHelper,
            IProjectBuilder projectBuilder,
            IIOUtilities ioUtilities
            )
        {
            _nugetHelper = nugetHelper;
            _projectBuilder = projectBuilder;
            _ioUtilities = ioUtilities;
        }

        public void Setup(string dependentProjectContents, string nupkgPath, Action<string> projectPathCallback)
        {
            var doc = XDocument.Parse(dependentProjectContents);
            _nugetHelper.Setup(nupkgPath, doc);

            _dependentProjectDirectory = _ioUtilities.CreateTempDirectory();
            var projectPath = _ioUtilities.SaveXDocumentToDirectory(doc, _dependentProjectDirectory, "DependentProject.csproj");
            doc.Save(projectPath);
            projectPathCallback(projectPath);

            _projectBuilder.Build(projectPath);
        }

        public void TearDown()
        {
            try
            {
                _nugetHelper.TearDown();
                if (_dependentProjectDirectory != null)
                {
                    Directory.Delete(_dependentProjectDirectory, true);
                }
            }
            catch
            {
                /* Ignore cleanup errors */
            }
        }

    }
}