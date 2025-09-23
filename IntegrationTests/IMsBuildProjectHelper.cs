using System.Xml.Linq;

namespace IntegrationTests
{
    internal interface IMsBuildProjectHelper
    {
        void AddPackageReference(XDocument doc, string nupkgPath);
        void InsertRestoreSourceAndRestorePackagesPath(XDocument project, string restoreSource, string restorePackagesPath);
    }
}