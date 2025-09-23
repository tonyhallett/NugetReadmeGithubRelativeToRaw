using System.Xml.Linq;

namespace IntegrationTests
{
    internal class MsBuildProjectHelper : IMsBuildProjectHelper
    {
        private static XElement GetRestorePropertyGroup(string restoreSource, string restorePackagesPath)
        {
            return new XElement("PropertyGroup", 
                new XElement("RestoreSources", $"{restoreSource};"),
                new XElement("RestorePackagesPath",restorePackagesPath));
        }

        public void InsertRestoreSourceAndRestorePackagesPath(XDocument project, string restoreSource, string restorePackagesPath)
        {
            project.Root!.AddFirst(GetRestorePropertyGroup(restoreSource, restorePackagesPath));
        }

        public void AddPackageReference(XDocument doc, string nupkgPath)
        {
            var (packageId, version) = GetPackageIdAndVersionFromNupkgPath(nupkgPath);
            var itemGroup = new XElement("ItemGroup",
                new XElement("PackageReference",
                    new XAttribute("Include", packageId),
                    new XAttribute("Version", version)
                )
            );
            doc.Root!.Add(itemGroup);
        }

        private static (string packageId, string version) GetPackageIdAndVersionFromNupkgPath(string nupkgPath)
        {
            var fileName = Path.GetFileNameWithoutExtension(nupkgPath);
            var packageNameParts = new List<string>();
            var versionParts = new List<string>();
            var foundMajor = false;
            var parts = fileName.Split('.');
            foreach (var part in parts)
            {
                if (foundMajor)
                {
                    versionParts.Add(part);
                }
                else
                {
                    if (int.TryParse(part, out _))
                    {
                        foundMajor = true;
                        versionParts.Add(part);
                    }
                    else
                    {
                        packageNameParts.Add(part);
                    }
                }
            }
            
            var packageId = string.Join(".", packageNameParts.ToArray());
            var version = string.Join(".", versionParts.ToArray());
            return (packageId, version);
        }
    }
}