using System.IO.Compression;

namespace IntegrationTests
{
    internal static class NupkgReadmeReader
    {
        public static string Read(DirectoryInfo directoryInfo, string zipEntryName)
        {
            var dependentNuGetPath = GetDependentNuGetPath(directoryInfo);
            using var zip = ZipFile.OpenRead(dependentNuGetPath);

            // nuget always stores readme at the root of the package
            var entry = zip.GetEntry(zipEntryName);

            using var reader = new StreamReader(entry!.Open());
            return reader.ReadToEnd();
        }

        private static string GetDependentNuGetPath(DirectoryInfo directoryInfo) => directoryInfo.GetFiles("*.nupkg", SearchOption.AllDirectories).First().FullName;
    }
}