using AngleSharp.Html.Dom;
using Moq;
using NugetReadmeGithubRelativeToRaw.Rewriter;

namespace UnitTests
{
    internal class ImgSrcAlt_Tests
    {
        [Test]
        public void Should_Be_Null_When_No_Src()
        {
            var mockHtmlImageElement = new Mock<IHtmlImageElement>();
            mockHtmlImageElement.Setup(ie => ie.GetAttribute("alt")).Returns((string?)null);
            
            Assert.That(ImgSrcAlt.TryGet(mockHtmlImageElement.Object), Is.Null);
        }

        [Test]
        public void Should_Have_Empty_Alt_When_No_Alt_Attribute()
        {
            var mockHtmlImageElement = new Mock<IHtmlImageElement>();
            mockHtmlImageElement.Setup(ie => ie.GetAttribute("src")).Returns("src");
            
            Assert.That(ImgSrcAlt.TryGet(mockHtmlImageElement.Object)!.Alt, Is.Empty);
        }

        [Test]
        public void Should_Have_Attributes_Alt_Src()
        {
            var mockHtmlImageElement = new Mock<IHtmlImageElement>();
            mockHtmlImageElement.Setup(ie => ie.GetAttribute("src")).Returns("src");
            mockHtmlImageElement.Setup(ie => ie.GetAttribute("alt")).Returns("alt");

            var imgSrcAlt = ImgSrcAlt.TryGet(mockHtmlImageElement.Object)!;

            Assert.Multiple(() =>
            {
                Assert.That(imgSrcAlt.Alt, Is.EqualTo("alt"));
                Assert.That(imgSrcAlt.Src, Is.EqualTo("src"));
            });
        }
    }
}
