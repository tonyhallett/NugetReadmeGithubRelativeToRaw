using NugetReadmeGithubRelativeToRaw.Rewriter;

namespace UnitTests
{
    internal class ReadmeRewriter_RemoveReplacer_Tests
    {
        private RemoveReplacer _removeReplacer = new(new RemoveReplaceRegexesFactory());

        [SetUp]
        public void Setup() => _removeReplacer = new RemoveReplacer(new RemoveReplaceRegexesFactory());

        [Test]
        public void Should_Not_When_No_Regexes_From_RemoveReplaceSettings()
        {
            var readMeContent = @"
readme not looked at
";
            var removeReplaceSettings = new RemoveReplaceSettings(null, []);
            var notReplaced = _removeReplacer.RemoveReplace(readMeContent, removeReplaceSettings);
            Assert.That(notReplaced, Is.EqualTo(readMeContent));
        }

        [Test]
        public void Should_Remove_Commented_Sections()
        {
            var readMeContent = @"
This is visible
<!-- remove-start -->
This is removed
<!-- remove-end -->
This is also visible
";
            var removeCommentIdentifiers = new RemoveCommentIdentifiers("remove-start", "remove-end");
            var removeReplaceSettings = new RemoveReplaceSettings(removeCommentIdentifiers, []);
            var rewrittenReadMe = _removeReplacer.RemoveReplace(readMeContent, removeReplaceSettings)!;

            var expectedReadMeContent = @"
This is visible
This is also visible
";

            Assert.That(rewrittenReadMe, Is.EqualTo(expectedReadMeContent));
        }

        [Test]
        public void Should_Remove_With_Regex()
        {
            var readMeContent = @"
This is visible
# Remove 1
This is removed
# Remove 2
This is also visible
";

            RemovalOrReplacement replacement = new RemovalOrReplacement(CommentOrRegex.Regex, "# Remove 1", "# Remove 2", null);
            var removeReplaceSettings = new RemoveReplaceSettings(null, [replacement]);
            var rewrittenReadMe = _removeReplacer.RemoveReplace(readMeContent, removeReplaceSettings)!;

            var expectedReadMeContent = @"
This is visible
This is also visible
";

            Assert.That(rewrittenReadMe, Is.EqualTo(expectedReadMeContent));
        }

        [Test]
        public void Should_Replace_With_Regex()
        {
            var readMeContent = @"
This is visible
# Remove 1
This is removed
# Remove 2
This is also visible
";

            RemovalOrReplacement replacement = new RemovalOrReplacement(CommentOrRegex.Regex, "# Remove 1", "# Remove 2", "Replaced Text");
            var removeReplaceSettings = new RemoveReplaceSettings(null, [replacement]);
            var rewrittenReadMe = _removeReplacer.RemoveReplace(readMeContent, removeReplaceSettings)!;

            var expectedReadMeContent = @"
This is visible
Replaced Text
This is also visible
";

            Assert.That(rewrittenReadMe, Is.EqualTo(expectedReadMeContent));
        }

        [Test]
        public void Should_Replace_With_Regex_Multiple()
        {
            var readMeContent = @"
This is visible
# Remove 1
This is removed
# Remove 2
This is also visible
# Remove A
This is also removed
# Remove B
and so is this
";

            RemovalOrReplacement replacement1 = new RemovalOrReplacement(CommentOrRegex.Regex, "# Remove 1", "# Remove 2", "Replaced Text");
            RemovalOrReplacement replacement2 = new RemovalOrReplacement(CommentOrRegex.Regex, "# Remove A", "# Remove B", "Replaced Text 2");
            var removeReplaceSettings = new RemoveReplaceSettings(null, [replacement1, replacement2]);
            var rewrittenReadMe = _removeReplacer.RemoveReplace(readMeContent, removeReplaceSettings)!;

            var expectedReadMeContent = @"
This is visible
Replaced Text
This is also visible
Replaced Text 2
and so is this
";

            Assert.That(rewrittenReadMe, Is.EqualTo(expectedReadMeContent));
        }

        [Test]
        public void Should_Remove_Remaining_Commented()
        {
            var readMeContent = @"
This is visible
<!-- remove-start -->
This is removed
";
            var removeCommentIdentifiers = new RemoveCommentIdentifiers("remove-start", "remove-end");
            var removeReplaceSettings = new RemoveReplaceSettings(removeCommentIdentifiers, []);
            var rewrittenReadMe = _removeReplacer.RemoveReplace(readMeContent, removeReplaceSettings)!;

            var expectedReadMeContent = @"
This is visible
";

            Assert.That(rewrittenReadMe, Is.EqualTo(expectedReadMeContent));
        }

        [Test]
        public void Should_Remove_Remaining_Regex()
        {
            var readMeContent = @"
This is visible
# Remove
This is removed
";
            RemovalOrReplacement regexRemoval = new RemovalOrReplacement(CommentOrRegex.Regex, "# Remove", null, null);
            var removeReplaceSettings = new RemoveReplaceSettings(null, [regexRemoval]);
            var rewrittenReadMe = _removeReplacer.RemoveReplace(readMeContent, removeReplaceSettings)!;

            var expectedReadMeContent = @"
This is visible
";

            Assert.That(rewrittenReadMe, Is.EqualTo(expectedReadMeContent));
        }

        [Test]
        public void Should_Replace_Remaining_Regex()
        {
            var readMeContent = @"
This is visible
# Replace remaining
This is removed
";
            RemovalOrReplacement regexRemoval = new RemovalOrReplacement(CommentOrRegex.Regex, "# Replace remaining", null, "Replaced");
            var removeReplaceSettings = new RemoveReplaceSettings(null, [regexRemoval]);
            var rewrittenReadMe = _removeReplacer.RemoveReplace(readMeContent, removeReplaceSettings)!;

            var expectedReadMeContent = @"
This is visible
Replaced";

            Assert.That(rewrittenReadMe, Is.EqualTo(expectedReadMeContent));
        }

        [Test]
        public void Should_Replace_Remaining_Comment()
        {
            var readMeContent = @"
This is visible
<!-- Replace remaining -->
This is removed
";
            RemovalOrReplacement commentRemoval = new RemovalOrReplacement(CommentOrRegex.Comment, "Replace remaining", null, "Replaced");
            var removeReplaceSettings = new RemoveReplaceSettings(null, [commentRemoval]);
            var rewrittenReadMe = _removeReplacer.RemoveReplace(readMeContent, removeReplaceSettings)!;

            var expectedReadMeContent = @"
This is visible
Replaced";

            Assert.That(rewrittenReadMe, Is.EqualTo(expectedReadMeContent));
        }
    }
}