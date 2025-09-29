using Moq;
using NugetReadmeGithubRelativeToRaw;

namespace UnitTests
{
    internal class RemoveReplaceSettingsProvider_Tests
    {
        [Test]
        public void Should_Provide_Null_When_Not_Specified()
        {
            var removeReplaceSettings = new RemoveReplaceSettingsProvider(new Mock<IIOHelper>().Object).Provide(null, null).Settings;

            Assert.That(removeReplaceSettings, Is.Null);
        }

        [TestCase("start;end")]
        [TestCase(" start ; end ")]
        public void Should_Provide_RemoveCommentIdentifiers_When_Specified(string removeCommentIdentifiersMsBuild)
        {
            var removeCommentIdentifiers = new RemoveReplaceSettingsProvider(new Mock<IIOHelper>().Object).Provide(null, removeCommentIdentifiersMsBuild).Settings!.RemoveCommentIdentifiers;
            
            Assert.Multiple(() =>
            {
                Assert.That(removeCommentIdentifiers!.Start, Is.EqualTo("start"));
                Assert.That(removeCommentIdentifiers!.End, Is.EqualTo("end"));
            });
        }

        [TestCase("start", RemoveReplaceSettingsProvider.NumPartsError)]
        [TestCase("start;end;", RemoveReplaceSettingsProvider.NumPartsError)]
        [TestCase("same;same", RemoveReplaceSettingsProvider.SamePartsError)]
        [TestCase(" ;end", RemoveReplaceSettingsProvider.EmptyPartsError)]
        public void Should_Have_Error_For_Invalid_RemoveCommentIdentifiers(string removeCommentIdentifiersMsBuild, string expectedError)
        {
            var result = new RemoveReplaceSettingsProvider(new Mock<IIOHelper>().Object)
                .Provide(null, removeCommentIdentifiersMsBuild);

            Assert.Multiple(() =>
            {
                Assert.That(result.Settings, Is.Null);
                Assert.That(result.Errors!.Single(), Is.EqualTo(expectedError));
            });
        }
    }
}
