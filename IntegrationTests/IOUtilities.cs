using System.Xml.Linq;

namespace IntegrationTests
{
    public class IOUtilities : IIOUtilities
    {
        public static IOUtilities Instance { get; } = new IOUtilities();

        public string CreateTempDirectory()
        {
            return Directory.CreateTempSubdirectory().FullName;
        }

        public void DeleteDirectory(string directoryPath)
        {
            Directory.Delete(directoryPath);
        }

        public string SaveXDocumentToDirectory(XDocument doc, string directory, string fileName)
        {
            var path = Path.Combine(directory, fileName);
            doc.Save(path);
            return path;
        }
    }
}
