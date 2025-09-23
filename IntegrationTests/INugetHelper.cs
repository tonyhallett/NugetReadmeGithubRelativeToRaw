using System.Xml.Linq;

namespace IntegrationTests
{
    public interface INugetHelper
    {
        void Setup(string nupkgPath, XDocument projectDocument);
        void TearDown();
    }
}
