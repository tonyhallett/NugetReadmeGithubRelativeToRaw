using NugetReadmeGithubRelativeToRaw;
using NugetReadmeGithubRelativeToRaw.Rewriter;

namespace UnitTests
{
    internal class RemoveCommentsIdentifiers_Test
    {
        private RemoveCommentsIdentifiersParser _removeCommentsIdentifiersParser;

        private sealed class RemoveCommentsMessageProvider : IMessageProvider
        {
            public const string FormatError = "formaterror";
            public const string SameStartEndError = "samestartenderror";

            public string CouldNotParseRepositoryUrl(string url)
            {
                throw new NotImplementedException();
            }

            public string CouldNotParseRewriteTagsOptionsUsingDefault(string propertyValue, RewriteTagsOptions defaultRewriteTagsOptions)
            {
                throw new NotImplementedException();
            }

            public string MissingReadmeAsset(string missingReadmeAsset)
            {
                throw new NotImplementedException();
            }

            public string ReadmeFileDoesNotExist(string readmeFilePath)
            {
                throw new NotImplementedException();
            }

            public string ReadmeHasUnsupportedHTML()
            {
                throw new NotImplementedException();
            }

            public string RemoveCommentsIdentifiersFormat() => FormatError;

            public string RemoveCommentsIdentifiersSameStartEnd() => SameStartEndError;

            public string RequiredMetadata(string metadataName, string itemSpec)
            {
                throw new NotImplementedException();
            }

            public string SameStartEndMetadata(string itemSpec)
            {
                throw new NotImplementedException();
            }

            public string UnsupportedCommentOrRegex(string itemSpec)
            {
                throw new NotImplementedException();
            }

            public string UnsupportedImageDomain(string imageDomain)
            {
                throw new NotImplementedException();
            }
        }


        [SetUp]
        public void SetUp()
        {
            _removeCommentsIdentifiersParser = new RemoveCommentsIdentifiersParser(new RemoveCommentsMessageProvider());
        }

        [TestCase((string?)null)]
        [TestCase("")]
        public void Should_Be_Null_When_Null_Or_Empty(string? nullOrEmpty)
        {
            var addError = new CollectingAddError();
            var removeCommentIdentifiers = _removeCommentsIdentifiersParser.Parse(nullOrEmpty, addError);

            Assert.Multiple(() =>
            {
                Assert.That(addError.Errors, Is.Empty);
                Assert.That(removeCommentIdentifiers, Is.Null);
            });
            
        }


        [TestCase("start", true)]
        [TestCase("start;end;", true)]
        [TestCase("same;same", false)]
        [TestCase(" ;end", true)]
        public void Should_Have_Error_For_Invalid_RemoveCommentIdentifiers(string removeCommentIdentifiersMsBuild, bool isFormatError)
        {
            var expectedError = isFormatError ? RemoveCommentsMessageProvider.FormatError : RemoveCommentsMessageProvider.SameStartEndError;
            var addError = new CollectingAddError();
            var parsed = _removeCommentsIdentifiersParser.Parse(removeCommentIdentifiersMsBuild, addError);

            Assert.Multiple(() =>
            {
                Assert.That(parsed, Is.Null);
                Assert.That(addError.Single(), Is.EqualTo(expectedError));
            });
        }

        [Test]
        public void Should_Parse_Valid_RemoveCommentIdentifiers()
        {
            var removeCommentIdentifiersMsBuild = " start ; end ";
            var addError = new CollectingAddError();
            var removeCommentIdentifiers = _removeCommentsIdentifiersParser.Parse(removeCommentIdentifiersMsBuild, addError);
            Assert.Multiple(() =>
            {
                Assert.That(addError.Errors, Is.Empty);
                Assert.That(removeCommentIdentifiers!.Start, Is.EqualTo("start"));
                Assert.That(removeCommentIdentifiers.End, Is.EqualTo("end"));
            });
        }
    }
}
