using AngleSharp.Html.Dom;

namespace NugetReadmeGithubRelativeToRaw
{
    public class ReplaceableATag
    {
        public ReplaceableATag(string textContent, string href)
        {
            TextContent = textContent;
            Href = href;
        }

        public string TextContent { get; }

        public string Href { get; }

        public static ReplaceableATag? TryGet(IHtmlAnchorElement anchorElement)
        {
            var href = anchorElement.GetAttribute("href");
            var download = anchorElement.GetAttribute("download");
            var textContent = anchorElement.TextContent;
            if (!string.IsNullOrWhiteSpace(href) && string.IsNullOrWhiteSpace(download) && !string.IsNullOrWhiteSpace(textContent))
            {
                return new ReplaceableATag(textContent, href!);
            }
            return null;
        }
    }
}
