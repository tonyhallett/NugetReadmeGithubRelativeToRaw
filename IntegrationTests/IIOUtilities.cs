using System.Xml.Linq;

namespace IntegrationTests
{
    internal interface IIOUtilities
    {
        string CreateTempDirectory();
        void DeleteDirectory(string localFeedPath);
        string SaveXDocumentToDirectory(XDocument doc, string directory, string fileName);
    }
}
