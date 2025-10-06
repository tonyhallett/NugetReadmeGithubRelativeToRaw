using AngleSharp.Html.Dom;

namespace NugetReadmeGithubRelativeToRaw.Rewriter
{
    internal class ImgSrcAlt
    {
        public ImgSrcAlt(string src, string? alt)
        {
            Src = src;
            Alt = alt ?? "";
        }

        public static ImgSrcAlt? TryGet(IHtmlImageElement imgElement)
        {
            var src = imgElement.GetAttribute("src");
            var alt = imgElement.GetAttribute("alt");
            if (!string.IsNullOrWhiteSpace(src))
            {
                return new ImgSrcAlt(src!, alt);
            }
            return null;
        }

        public string Src { get; }
        public string Alt { get; }
    }
}
