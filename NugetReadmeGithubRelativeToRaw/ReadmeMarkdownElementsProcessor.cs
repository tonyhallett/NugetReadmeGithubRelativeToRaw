using System;
using System.Collections.Generic;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using AngleSharp.Html.Dom;

namespace NugetReadmeGithubRelativeToRaw
{
    internal class ReadmeMarkdownElementsProcessor : IReadmeMarkdownElementsProcessor
    {
        private readonly INugetImageDomainValidator nugetImageDomainValidator;
        private readonly IGitHubUrlHelper gitHubUrlHelper;

        public ReadmeMarkdownElementsProcessor(INugetImageDomainValidator nugetImageDomainValidator, IGitHubUrlHelper gitHubUrlHelper)
        {
            this.nugetImageDomainValidator = nugetImageDomainValidator;
            this.gitHubUrlHelper = gitHubUrlHelper;
        }

        public IMarkdownElementsProcessResult Process(RelevantMarkdownElements relevantMarkdownElements, string rawUrl, RewriteTagsOptions rewriteTagsOptions)
        {
            var markdownElementsProcessResult = new MarkdownElementsProcessResult();
            ProcessLinkInlines(relevantMarkdownElements.LinkInlines, markdownElementsProcessResult, rawUrl);
            ProcessHtmlBlocks(relevantMarkdownElements.HtmlBlocks, markdownElementsProcessResult, rawUrl, rewriteTagsOptions);
            ProcessHtmlInlines(relevantMarkdownElements.HtmlInlines, markdownElementsProcessResult, rawUrl, rewriteTagsOptions);
            return markdownElementsProcessResult;
        }

        private void ProcessHtmlInlines(IEnumerable<HtmlInline> htmlInlines, MarkdownElementsProcessResult markdownElementsProcessResult, string rawUrl, RewriteTagsOptions rewriteTagsOptions)
        {
            foreach (var htmlInline in htmlInlines)
            {
                if (htmlInline.Tag == "<br/>" && rewriteTagsOptions.HasFlag(RewriteTagsOptions.RewriteBrTags))
                {
                    markdownElementsProcessResult.AddSourceReplacement(htmlInline.Span, "\\");
                }
            }
        }

        private void ProcessHtmlBlocks(
            IEnumerable<HtmlBlock> htmlBlocks, 
            MarkdownElementsProcessResult markdownElementsProcessResult, 
            string rawUrl, 
            RewriteTagsOptions rewriteTagsOptions)
        {
            foreach (var htmlBlock in htmlBlocks)
            {
                var root = HtmlBlockTransformer.TransformToDom(htmlBlock);

                switch (root.NodeName.ToLowerInvariant())
                {
                    case "a" when rewriteTagsOptions.HasFlag(RewriteTagsOptions.RewriteATags):
                        var anchorElement = root as IHtmlAnchorElement;
                        var href = anchorElement!.GetAttribute("href");
                        var download = anchorElement!.GetAttribute("download");
                        var innerText = root.TextContent ?? string.Empty;
                        if (!string.IsNullOrWhiteSpace(href) && string.IsNullOrWhiteSpace(download))
                        {
                            var md = $"[{innerText}]({href})";
                            AddSourceReplacement(md);
                        }
                        break;

                    case "img" when rewriteTagsOptions.HasFlag(RewriteTagsOptions.RewriteImgTagsForSupportedDomains):
                        var imgElement = root as IHtmlImageElement;
                        var src = imgElement!.GetAttribute("src");
                        var alt = imgElement!.GetAttribute("alt") ?? string.Empty;
                        if (!string.IsNullOrWhiteSpace(src) && !string.IsNullOrWhiteSpace(alt))
                        {
                            // todo relative to absolute
                            // unsupported domains
                            var md = $"![{alt}]({src})";
                            AddSourceReplacement(md);
                            //if (IsTrustedImage(src))
                            //{
                            //    var md = $"![{alt}]({src})";
                            //    replacements.Add((offset, offset + htmlText.Length - 1, md));
                            //}
                            //else
                            //{
                            //    unsupportedDomains.Add(new Uri(src).Host);
                            //}
                        }
                        break;
                        // ignore other HTML
                }

                void AddSourceReplacement(string replacementText)
                {
                    markdownElementsProcessResult.AddSourceReplacement(htmlBlock.Span, replacementText);
                }
            }
        }

        private void ProcessLinkInlines(
            IEnumerable<LinkInline> linkInlines, 
            MarkdownElementsProcessResult markdownElementsProcessResult, 
            string rawUrl)
        {
            foreach (var linkInline in linkInlines)
            {
                if (linkInline.IsImage)
                {
                    var absoluteUri = gitHubUrlHelper.GetAbsoluteUri(linkInline.Url);
                    if (absoluteUri != null)
                    {
                        if (!nugetImageDomainValidator.IsTrustedImageDomain(absoluteUri.OriginalString))
                        {
                            markdownElementsProcessResult.AddUnsupportedImageDomain(absoluteUri.Host);
                            continue;
                        }
                    }
                }
                
                var urlSpan = linkInline.Reference != null ? linkInline.Reference.UrlSpan : linkInline.UrlSpan;
                ProcessInlineUrl(linkInline.Url, rawUrl, urlSpan, markdownElementsProcessResult.AddSourceReplacement);
            }
        }

        private void ProcessInlineUrl(string? url, string rawUrl, SourceSpan span, Action<SourceSpan, string> addSourceReplacement)
        {
            var githubAbsoluteUrl = gitHubUrlHelper.GetGithubAbsoluteUrl(url, rawUrl);
            if (githubAbsoluteUrl != null)
            {
                addSourceReplacement(span, githubAbsoluteUrl);
            }
        }
    }
}
