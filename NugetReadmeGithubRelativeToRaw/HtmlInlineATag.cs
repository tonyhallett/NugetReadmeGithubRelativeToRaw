using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace NugetReadmeGithubRelativeToRaw
{
    internal class HtmlInlineATag
    {
        public HtmlInlineATag(string text, SourceSpan span)
        {
            Text = text;
            Span = span;
        }

        public string Text { get; }
        public SourceSpan Span { get; }

        public static HtmlInlineATag? TryCreate(HtmlInline htmlInline)
        {
            if(!IsATagStart(htmlInline))
            {
                return null;
            }

            var fullText = htmlInline.Tag;
            if (htmlInline.NextSibling is LiteralInline literalInline && literalInline.NextSibling is HtmlInline closingATag && IsATagEnd(closingATag))
            {
                fullText += literalInline.Content;
                fullText += closingATag.Tag;
                var span = htmlInline.Span.Combine(literalInline.Span, closingATag.Span);
                return new HtmlInlineATag(fullText, span);
            }

            return null;
        }


        private static bool IsATagStart(HtmlInline htmlInline)
        {
            var tag = htmlInline.Tag.Trim().ToLower();
            return tag.StartsWith("<a");
        }

        private static bool IsATagEnd(HtmlInline htmlInline)
        {
            var tag = htmlInline.Tag.Trim().ToLower();
            return tag == "</a>";
        }
    }
}
