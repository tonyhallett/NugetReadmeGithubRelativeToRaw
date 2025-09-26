using Moq;
using NugetReadmeGithubRelativeToRaw;

namespace UnitTests
{
    internal class NugetImageDomainValidator_Tests
    {
        private Mock<INugetTrustedImageDomains> _mockNugetTrustedImageDomains;
        private Mock<INugetGitHubBadgeValidator> _mockNugetGitHubBadgeValidator;
        private NugetImageDomainValidator _nugetImageDomainValidator;

        [SetUp]
        public void SetUp()
        {
            _mockNugetTrustedImageDomains = new Mock<INugetTrustedImageDomains>();
            _mockNugetGitHubBadgeValidator = new Mock<INugetGitHubBadgeValidator>();
            _nugetImageDomainValidator = new NugetImageDomainValidator(_mockNugetTrustedImageDomains.Object, _mockNugetGitHubBadgeValidator.Object);
        }

        [Test]
        public void Should_Not_Be_Trusted_If_Relative_Url()
        {
            Assert.That(_nugetImageDomainValidator.IsTrustedImageDomain("/relative"), Is.False);
            VerifyDependencies(false);
        }

        [Test]
        public void Should_Not_Be_Trusted_For_Non_Http_Or_Https()
        {
            var ftpUrl = "ftp://example.com/image.png";
            Assert.That(_nugetImageDomainValidator.IsTrustedImageDomain(ftpUrl), Is.False);
            VerifyDependencies(false);
        }

        
        [TestCase(true)]
        [TestCase(false)]
        public void Should_Be_Trusted_If_The_Host_Is_A_Trusted_Domain(bool https)
        {
            var protocol = https ? "https" : "http";
            _mockNugetTrustedImageDomains.Setup(nugetTrustedImageDomains => nugetTrustedImageDomains.IsImageDomainTrusted("trusted.com")).Returns(true);
            Assert.That(_nugetImageDomainValidator.IsTrustedImageDomain($"{protocol}://trusted.com/page"), Is.True);
        }

        [Test]
        public void Should_Be_Trusted_If_The_Url_Is_A_Valid_GitHub_Badge()
        {
            var validBadgeUrl = "https://validbadge";
            _mockNugetGitHubBadgeValidator.Setup(nugetGitHubBadgeValidator => nugetGitHubBadgeValidator.Validate(validBadgeUrl)).Returns(true);
            Assert.That(_nugetImageDomainValidator.IsTrustedImageDomain(validBadgeUrl), Is.True);
        }

        [Test]
        public void Should_Not_Be_Trusted_If_Is_Neither_A_Trusted_Domain_Or_GitHub_Badge_Url()
        {
            Assert.That(_nugetImageDomainValidator.IsTrustedImageDomain("https://example.com/page"), Is.False);
            VerifyDependencies(true);
        }

        private void VerifyDependencies(bool called)
        {
            var times = called ? Times.Once() : Times.Never();
            _mockNugetTrustedImageDomains.Verify(x => x.IsImageDomainTrusted(It.IsAny<string>()), times);
            _mockNugetGitHubBadgeValidator.Verify(x => x.Validate(It.IsAny<string>()), times);
        }
    }
}
