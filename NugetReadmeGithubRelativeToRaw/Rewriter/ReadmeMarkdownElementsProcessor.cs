using System;
using System.Collections.Generic;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using AngleSharp.Html.Dom;
using NugetReadmeGithubRelativeToRaw.Rewriter.Validation;

namespace NugetReadmeGithubRelativeToRaw.Rewriter
{
    internal class ReadmeMarkdownElementsProcessor : IReadmeMarkdownElementsProcessor
    {
        private readonly INugetImageDomainValidator nugetImageDomainValidator;
        private readonly IGitHubUrlHelper gitHubUrlHelper;
        private readonly IHtmlFragmentParser htmlFragmentParser;

        public ReadmeMarkdownElementsProcessor(
            INugetImageDomainValidator nugetImageDomainValidator, 
            IGitHubUrlHelper gitHubUrlHelper,
            IHtmlFragmentParser htmlFragmentParser
            )
        {
            this.nugetImageDomainValidator = nugetImageDomainValidator;
            this.gitHubUrlHelper = gitHubUrlHelper;
            this.htmlFragmentParser = htmlFragmentParser;
        }

        public IMarkdownElementsProcessResult Process(RelevantMarkdownElements relevantMarkdownElements, OwnerRepoRefReadmePath ownerRepoRefReadmePath, RewriteTagsOptions rewriteTagsOptions)
        {
            var markdownElementsProcessResult = new MarkdownElementsProcessResult();
            ProcessLinkInlines(relevantMarkdownElements.LinkInlines, markdownElementsProcessResult, ownerRepoRefReadmePath);
            ProcessHtmlBlocks(relevantMarkdownElements.HtmlBlocks, markdownElementsProcessResult, ownerRepoRefReadmePath, rewriteTagsOptions);
            ProcessHtmlInlines(relevantMarkdownElements.HtmlInlines, markdownElementsProcessResult, ownerRepoRefReadmePath, rewriteTagsOptions);
            return markdownElementsProcessResult;
        }

        private void ProcessHtmlInlines(IEnumerable<HtmlInline> htmlInlines, MarkdownElementsProcessResult markdownElementsProcessResult, OwnerRepoRefReadmePath ownerRepoRefReadmePath, RewriteTagsOptions rewriteTagsOptions)
        {
            foreach (var htmlInline in htmlInlines)
            {
                if (rewriteTagsOptions.HasFlag(RewriteTagsOptions.RewriteBrTags) && htmlInline.IsBrTag())
                {
                    markdownElementsProcessResult.AddSourceReplacement(htmlInline.Span, "\\");
                }
                else if (rewriteTagsOptions.HasFlag(RewriteTagsOptions.RewriteATags) && HtmlInlineATag.TryCreate(htmlInline) is HtmlInlineATag htmlInlineATag)
                {
                    var anchorElement = htmlFragmentParser.Parse(htmlInlineATag.Text) as IHtmlAnchorElement;
                    var href = anchorElement!.GetAttribute("href");
                    if (href != null && HrefIsValid(href))
                    {
                        href = gitHubUrlHelper.GetAbsoluteOrGithubAbsoluteUrl(href, ownerRepoRefReadmePath, false);
                        markdownElementsProcessResult.AddSourceReplacement(htmlInlineATag.Span, $"[{anchorElement!.TextContent}]({href})");
                    }
                }
            }

            bool HrefIsValid(string href)
            {
                href = href.Trim(); 
                return !string.IsNullOrWhiteSpace(href)
                    && !href.StartsWith("#", StringComparison.OrdinalIgnoreCase)
                    && !href.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase)
                    && !href.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase)
                    && !href.StartsWith("tel:", StringComparison.OrdinalIgnoreCase);
            }
        }

        private void ProcessHtmlBlocks(
            IEnumerable<HtmlBlock> htmlBlocks, 
            MarkdownElementsProcessResult markdownElementsProcessResult,
            OwnerRepoRefReadmePath ownerRepoRefReadmePath, 
            RewriteTagsOptions rewriteTagsOptions)
        {
            foreach (var htmlBlock in htmlBlocks)
            {
                var root = htmlFragmentParser.Parse(htmlBlock);

                switch (root.NodeName.ToLowerInvariant())
                {
                    case "img" when rewriteTagsOptions.HasFlag(RewriteTagsOptions.RewriteImgTagsForSupportedDomains) && DefinedSrcAlt.TryGet((root as IHtmlImageElement)!) is DefinedSrcAlt srcAlt:
                        var src = srcAlt.Src;
                        if (gitHubUrlHelper.GetAbsoluteUri(src) is Uri absoluteUri){
                            if (!nugetImageDomainValidator.IsTrustedImageDomain(absoluteUri.OriginalString))
                            {
                                markdownElementsProcessResult.AddUnsupportedImageDomain(absoluteUri.Host);
                                break;
                            }
                        }else
                        {
                            src = gitHubUrlHelper.GetGithubAbsoluteUrl(src, ownerRepoRefReadmePath, true)!;
                        }
                        var imgTagReplacement = $"![{srcAlt.Alt}]({src})";
                        AddSourceReplacement(imgTagReplacement);
                        break;
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
            OwnerRepoRefReadmePath ownerRepoRefReadmePath)
        {
            foreach (var linkInline in linkInlines)
            {
                if (linkInline.IsAutoLink)
                {
                    continue;
                }

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
                ProcessInlineUrl(linkInline.Url, linkInline.IsImage, ownerRepoRefReadmePath, urlSpan, markdownElementsProcessResult.AddSourceReplacement);
            }
        }

        private void ProcessInlineUrl(string? url,bool isImage, OwnerRepoRefReadmePath ownerRepoRefReadmePath, SourceSpan span, Action<SourceSpan, string> addSourceReplacement)
        {
            var githubAbsoluteUrl = gitHubUrlHelper.GetGithubAbsoluteUrl(url, ownerRepoRefReadmePath, isImage);
            if (githubAbsoluteUrl != null)
            {
                addSourceReplacement(span, githubAbsoluteUrl);
            }
        }
    }
}
