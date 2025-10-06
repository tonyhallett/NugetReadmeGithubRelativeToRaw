using System;
using Microsoft.Build.Framework;
using NugetReadmeGithubRelativeToRaw.MSBuildHelpers;
using NugetReadmeGithubRelativeToRaw.Rewriter;
using InputOutputHelper = NugetReadmeGithubRelativeToRaw.IOHelper;

namespace NugetReadmeGithubRelativeToRaw
{
    public class ReadmeRewriterTask : Microsoft.Build.Utilities.Task
    {
        internal const RewriteTagsOptions DefaultRewriteTagsOptions = Rewriter.RewriteTagsOptions.Error;

        [Required]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public string ProjectDirectoryPath { get; set; }
        
        [Required]
        public string OutputReadme { get; set; }
        [Required]
        public string RepositoryUrl { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public string? ReadmeRelativePath { get; set; }
        
        public string? RepositoryBranch { get; set; }

        public string? RewriteTagsOptions { get; set; }

        public string? RemoveCommentIdentifiers { get; set; }

        public ITaskItem[]? RemoveReplaceItems { get; set; }

        internal IIOHelper IOHelper { get; set; } = InputOutputHelper.Instance;

        internal IReadmeRelativeFileExistsFactory ReadmeRelativeFileExistsFactory { get; set; } = new ReadmeRelativeFileExistsFactory();

        internal IMessageProvider MessageProvider { get; set; } = NugetReadmeGithubRelativeToRaw.MessageProvider.Instance;

        internal IReadmeRewriter ReadmeRewriter { get; set; } = new ReadmeRewriter();

        internal IRemoveReplaceSettingsProvider RemoveReplaceSettingsProvider { get; set; } = new RemoveReplaceSettingsProvider(
            new MSBuildMetadataProvider(), 
            new RemoveCommentsIdentifiersParser(NugetReadmeGithubRelativeToRaw.MessageProvider.Instance),
            new RemovalOrReplacementProvider(InputOutputHelper.Instance, NugetReadmeGithubRelativeToRaw.MessageProvider.Instance),
            NugetReadmeGithubRelativeToRaw.MessageProvider.Instance
            );

        public override bool Execute()
        {
            var readmeRelativePath = ReadmeRelativePath ?? "readme.md";
            var readmePath = IOHelper.CombinePaths(ProjectDirectoryPath, readmeRelativePath);
            if (!IOHelper.FileExists(readmePath))
            {
                Log.LogError(MessageProvider.ReadmeFileDoesNotExist(readmePath));
            }
            else
            {
                TryRewrite(IOHelper.ReadAllText(readmePath), readmeRelativePath);
            }
            
            return !Log.HasLoggedErrors;
        }

        private void TryRewrite(string readmeContents, string readmeRelativePath)
        {
            var removeReplaceSettingsResult = RemoveReplaceSettingsProvider.Provide(
                RemoveReplaceItems, 
                RemoveCommentIdentifiers);
            if (removeReplaceSettingsResult.Errors.Count > 0)
            {
                foreach(var error in removeReplaceSettingsResult.Errors)
                {
                    Log.LogError(error);
                }
            }
            else
            {
                Rewrite(readmeContents, readmeRelativePath, removeReplaceSettingsResult.Settings);
            }
        }

        private void Rewrite(string readmeContents, string readmeRelativePath, RemoveReplaceSettings? removeReplaceSettings)
        {
            var readmeRelativeFileExists = ReadmeRelativeFileExistsFactory.Create(ProjectDirectoryPath, readmeRelativePath, IOHelper);
            var readmeRewriterResult = ReadmeRewriter.Rewrite(
                GetRewriteTagsOptions(),
                readmeContents,
                readmeRelativePath,
                RepositoryUrl,
                RepositoryBranch,
                removeReplaceSettings,
                readmeRelativeFileExists
                );


            foreach (var unsupportedImageDomain in readmeRewriterResult.UnsupportedImageDomains)
            {
                Log.LogError(MessageProvider.UnsupportedImageDomain(unsupportedImageDomain));
            }

            foreach (var missingReadmeAsset in readmeRewriterResult.MissingReadmeAssets)
            {
                Log.LogError(MessageProvider.MissingReadmeAsset(missingReadmeAsset));
            }

            if (readmeRewriterResult.HasUnsupportedHTML)
            {
                Log.LogError(MessageProvider.ReadmeHasUnsupportedHTML());
            }

            if (readmeRewriterResult.UnsupportedRepo)
            {
                Log.LogError(MessageProvider.CouldNotParseRepositoryUrl(RepositoryUrl));
            }

            if (!Log.HasLoggedErrors)
            {
                IOHelper.WriteAllText(OutputReadme, readmeRewriterResult.RewrittenReadme!);
            }
        }

        private RewriteTagsOptions GetRewriteTagsOptions()
        {
            var options = DefaultRewriteTagsOptions;
            if (RewriteTagsOptions != null)
            {
                if(Enum.TryParse(RewriteTagsOptions, out RewriteTagsOptions parsedOptions))
                {
                    options = parsedOptions;
                }
                else
                {
                    Log.LogWarning(MessageProvider.CouldNotParseRewriteTagsOptionsUsingDefault(RewriteTagsOptions, DefaultRewriteTagsOptions));
                }
            }
            return options;
        }
    }
}
