using System.Xml.Linq;

namespace IntegrationTests
{
    internal class NugetHelper : INugetHelper
    {
        private readonly IIOUtilities _ioUtilities;
        private readonly INugetAddCommand _nugetAddCommand;
        private readonly IMsBuildProjectHelper _msBuildProjectHelper;
        private string? _localFeedPath;

        public NugetHelper(IIOUtilities ioUtilities, INugetAddCommand nugetAddCommand, IMsBuildProjectHelper msBuildProjectHelper)
        {
            _ioUtilities = ioUtilities;
            _nugetAddCommand = nugetAddCommand;
            _msBuildProjectHelper = msBuildProjectHelper;
        }

        public void Setup(string nupkgPath, XDocument projectDocument)
        {
            _localFeedPath = _ioUtilities.CreateTempDirectory();
            this._nugetAddCommand.AddPackageToLocalFeed(nupkgPath, _localFeedPath);
            _msBuildProjectHelper.InsertRestoreSourceAndRestorePackagesPath(projectDocument, _localFeedPath, "$(BaseIntermediateOutputPath)\\packages");
            _msBuildProjectHelper.AddPackageReference(projectDocument, nupkgPath);
        }

        public void TearDown()
        {
            if (_localFeedPath != null)
            {
                try
                {
                    _ioUtilities.DeleteDirectory(_localFeedPath);
                }
                catch
                {
                    /* Ignore cleanup errors */
                }
            }
        }
    }
}