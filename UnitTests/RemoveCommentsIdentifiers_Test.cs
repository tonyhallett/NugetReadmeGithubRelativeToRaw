using NugetReadmeGithubRelativeToRaw;

namespace UnitTests
{
    internal class RemoveCommentsIdentifiers_Test
    {
        [TestCase("start", RemoveCommentsIdentifiersParser.NumPartsError)]
        [TestCase("start;end;", RemoveCommentsIdentifiersParser.NumPartsError)]
        [TestCase("same;same", RemoveCommentsIdentifiersParser.SamePartsError)]
        [TestCase(" ;end", RemoveCommentsIdentifiersParser.EmptyPartsError)]
        public void Should_Have_Error_For_Invalid_RemoveCommentIdentifiers(string removeCommentIdentifiersMsBuild, string expectedErrorFormat)
        {
            var expectedError = string.Format(expectedErrorFormat, nameof(ReadmeRewriterTask.RemoveCommentIdentifiers));
            var removeCommentIdentifiersParser = new RemoveCommentsIdentifiersParser();
            var errors = new List<string>();
            var parsed = removeCommentIdentifiersParser.Parse(removeCommentIdentifiersMsBuild, errors);

            Assert.That(parsed, Is.Null);
            Assert.That(errors!.Single(), Is.EqualTo(expectedError));
        }

        [Test]
        public void Should_Parse_Valid_RemoveCommentIdentifiers()
        {
            var removeCommentIdentifiersMsBuild = " start ; end ";
            var removeCommentIdentifiersParser = new RemoveCommentsIdentifiersParser();
            var errors = new List<string>();
            var removeCommentIdentifiers = removeCommentIdentifiersParser.Parse(removeCommentIdentifiersMsBuild, errors);
            Assert.Multiple(() =>
            {
                Assert.That(errors, Is.Empty);
                Assert.That(removeCommentIdentifiers!.Start, Is.EqualTo("start"));
                Assert.That(removeCommentIdentifiers.End, Is.EqualTo("end"));
            });
        }
    }
}
