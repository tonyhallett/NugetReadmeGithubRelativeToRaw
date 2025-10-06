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
        private readonly INuGetImageDomainValidator nugetImageDomainValidator;
        private readonly IRepoUrlHelper repoUrlHelper;
        private readonly IHtmlFragmentParser htmlFragmentParser;

        public ReadmeMarkdownElementsProcessor(
            INuGetImageDomainValidator nugetImageDomainValidator, 
            IRepoUrlHelper repoUrlHelper,
            IHtmlFragmentParser htmlFragmentParser
            )
        {
            this.nugetImageDomainValidator = nugetImageDomainValidator;
            this.repoUrlHelper = repoUrlHelper;
            this.htmlFragmentParser = htmlFragmentParser;
        }

        public IMarkdownElementsProcessResult Process(
            RelevantMarkdownElements relevantMarkdownElements, 
            RepoPaths? repoPaths, 
            RewriteTagsOptions rewriteTagsOptions,
            IReadmeRelativeFileExists readmeRelativeFileExists)
        {
            var markdownElementsProcessResult = new MarkdownElementsProcessResult();
            ProcessLinkInlines(relevantMarkdownElements.LinkInlines, markdownElementsProcessResult, repoPaths, readmeRelativeFileExists);
            ProcessHtmlBlocks(relevantMarkdownElements.HtmlBlocks, markdownElementsProcessResult, repoPaths, rewriteTagsOptions);
            if (repoPaths != null)
            {
                ProcessHtmlInlines(relevantMarkdownElements.HtmlInlines, markdownElementsProcessResult, repoPaths, rewriteTagsOptions);
            }

            return markdownElementsProcessResult;
        }

        private void ProcessHtmlInlines(
            IEnumerable<HtmlInline> htmlInlines, 
            MarkdownElementsProcessResult markdownElementsProcessResult, 
            RepoPaths repoPaths, 
            RewriteTagsOptions rewriteTagsOptions)
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
                        href = repoUrlHelper.GetAbsoluteOrRepoAbsoluteUrl(href, repoPaths, false);
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
            RepoPaths? repoPaths, 
            RewriteTagsOptions rewriteTagsOptions)
        {
            if (!rewriteTagsOptions.HasFlag(RewriteTagsOptions.RewriteImgTagsForSupportedDomains))
            {
                return;
            }

            // could have error for img without src / alt - and can you have missing alt in markdown
            htmlBlocks.SelectWithSourceNotNull(htmlBlock => htmlFragmentParser.Parse<IHtmlImageElement>(htmlBlock))
                .SelectWithSourceNotNull(htmlImageElement => ImgSrcAlt.TryGet(htmlImageElement))
                .ProcessSourceResult(Process);

            void Process(ImgSrcAlt srcAlt,HtmlBlock htmlBlock)
            {
                var src = srcAlt.Src;
                if (repoUrlHelper.GetAbsoluteUri(src) is Uri absoluteUri)
                {
                    if (!nugetImageDomainValidator.IsTrustedImageDomain(absoluteUri.OriginalString))
                    {
                        markdownElementsProcessResult.AddUnsupportedImageDomain(absoluteUri.Host);
                        return;
                    }
                }
                else
                {
                    if (repoPaths == null)
                    {
                        return;
                    }

                    src = repoUrlHelper.GetRepoAbsoluteUrl(src, repoPaths, true)!;
                }

                markdownElementsProcessResult.AddSourceReplacement(htmlBlock.Span, $"![{srcAlt.Alt}]({src})");
            }
        }

        private void ProcessLinkInlines(
            IEnumerable<LinkInline> linkInlines, 
            MarkdownElementsProcessResult markdownElementsProcessResult,
            RepoPaths? repoPaths,
            IReadmeRelativeFileExists readmeRelativeFileExists)
        {
            foreach (var linkInline in linkInlines)
            {
                if (IgnoreLinkInline(linkInline))
                {
                    continue;
                }

                var absoluteUri = repoUrlHelper.GetAbsoluteUri(linkInline.Url);
                if (linkInline.IsImage)
                {
                    if (absoluteUri != null)
                    {
                        if (!nugetImageDomainValidator.IsTrustedImageDomain(absoluteUri.OriginalString))
                        {
                            markdownElementsProcessResult.AddUnsupportedImageDomain(absoluteUri.Host);
                            continue;
                        }
                    }
                }
                if (absoluteUri == null && linkInline.Url != null)
                {
                    if (!readmeRelativeFileExists.Exists(linkInline.Url))
                    {
                        markdownElementsProcessResult.AddMissingReadmeAsset(linkInline.Url);
                        continue;
                    }
                }
                if (repoPaths != null)
                {
                    var urlSpan = linkInline.Reference != null ? linkInline.Reference.UrlSpan : linkInline.UrlSpan;
                    ProcessInlineUrl(linkInline.Url, linkInline.IsImage, repoPaths, urlSpan, markdownElementsProcessResult.AddSourceReplacement);
                }
            }
        }

        private bool IgnoreLinkInline(LinkInline linkInline)
        {
            if (linkInline.IsAutoLink || linkInline.Url == null)
            {
                return true;
            }

            var url = linkInline.Url.Trim();

            // ignore empty and fragments
            if (string.IsNullOrEmpty(url) || url.StartsWith("#", StringComparison.Ordinal))
            {
                return true;
            }

            return false;
        }

        private void ProcessInlineUrl(
            string? url,
            bool isImage, 
            RepoPaths repoPaths, 
            SourceSpan span, 
            Action<SourceSpan, string> addSourceReplacement)
        {
            var repoAbsoluteUrl = repoUrlHelper.GetRepoAbsoluteUrl(url, repoPaths, isImage);
            if (repoAbsoluteUrl != null)
            {
                addSourceReplacement(span, repoAbsoluteUrl);
            }
        }
    }
}
