using AngleSharp.Html.Dom;

namespace NugetReadmeGithubRelativeToRaw.Rewriter
{
    internal class DefinedSrcAlt
    {
        public DefinedSrcAlt(string src, string alt)
        {
            Src = src;
            Alt = alt;
        }

        public static DefinedSrcAlt? TryGet(IHtmlImageElement imgElement)
        {
            var src = imgElement.GetAttribute("src");
            var alt = imgElement.GetAttribute("alt");
            if (!string.IsNullOrWhiteSpace(src) && !string.IsNullOrWhiteSpace(alt))
            {
                return new DefinedSrcAlt(src!, alt!);
            }
            return null;
        }

        public string Src { get; }
        public string Alt { get; }
    }
}
