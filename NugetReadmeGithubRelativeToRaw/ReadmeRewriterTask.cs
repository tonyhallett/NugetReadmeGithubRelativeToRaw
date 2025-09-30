using System;
using Microsoft.Build.Framework;
using NugetReadmeGithubRelativeToRaw.MSBuildHelpers;
using NugetReadmeGithubRelativeToRaw.Rewriter;

namespace NugetReadmeGithubRelativeToRaw
{
    public class ReadmeRewriterTask : Microsoft.Build.Utilities.Task
    {
        internal const string UnsupportedImageDomainErrorFormat = "Unsupported image domain found in README: {0}";
        internal const string ReadmeFileDoesNotExistErrorFormat = "Readme file does not exist: {0}";
        internal const string CouldNotParseRepositoryUrlErrorFormat = "Could not parse the {0}: {1}";
        internal const RewriteTagsOptions DefaultRewriteTagsOptions = Rewriter.RewriteTagsOptions.All;
        internal const string CouldNotParseRewriteTagsOptionsWarningFormat = "Could not parse the {0}: {1}. Using the default: {2}";

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

        internal IIOHelper IOHelper { get; set; } = new IOHelper();

        internal IReadmeRewriter ReadmeRewriter { get; set; } = new ReadmeRewriter();

        internal IRemoveReplaceSettingsProvider RemoveReplaceSettingsProvider { get; set; } = new RemoveReplaceSettingsProvider(new IOHelper(), new MSBuildMetadataProvider(), new RemoveCommentsIdentifiersParser());

        public override bool Execute()
        {
            var readmeRelativePath = ReadmeRelativePath ?? "readme.md";
            var readmePath = IOHelper.CombinePaths(ProjectDirectoryPath, readmeRelativePath);
            if (!IOHelper.FileExists(readmePath))
            {
                LogFormatError(ReadmeFileDoesNotExistErrorFormat, readmePath);
            }
            else
            {
                TryRewrite(IOHelper.ReadAllText(readmePath), readmeRelativePath);
            }
            
            return !Log.HasLoggedErrors;
        }

        private void TryRewrite(string readmeContents, string readmeRelativePath)
        {
            var removeReplaceSettingsResult = RemoveReplaceSettingsProvider.Provide(RemoveReplaceItems, RemoveCommentIdentifiers);
            if(removeReplaceSettingsResult.Errors?.Count > 0)
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
            var readmeRewriterResult = ReadmeRewriter.Rewrite(readmeContents, readmeRelativePath, RepositoryUrl, RepositoryBranch, GetRewriteTagsOptions(), removeReplaceSettings);
            if (readmeRewriterResult != null)
            {
                ProcessReadmeWriteResult(readmeRewriterResult);
            }
            else
            {
                LogFormatError(CouldNotParseRepositoryUrlErrorFormat, nameof(RepositoryUrl), RepositoryUrl);
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
                    LogFormatWarning(CouldNotParseRewriteTagsOptionsWarningFormat, nameof(RewriteTagsOptions), RewriteTagsOptions, DefaultRewriteTagsOptions.ToString());
                }
            }
            return options;
        }

        private void ProcessReadmeWriteResult(ReadmeRewriterResult readmeRewriterResult)
        {
            if (readmeRewriterResult.UnsupportedImageDomains.Count > 0)
            {
                foreach (var unsupportedImageDomain in readmeRewriterResult.UnsupportedImageDomains)
                {
                    LogFormatError(UnsupportedImageDomainErrorFormat, unsupportedImageDomain);
                }
            }
            else
            {
                IOHelper.WriteAllText(OutputReadme, readmeRewriterResult.RewrittenReadme);
            }
        }

        private void LogFormatError(string format, params string[] args)
        {
            Log.LogError(string.Format(format, args));
        }

        private void LogFormatWarning(string format, params string[] args)
        {
            Log.LogWarning(string.Format(format, args));
        }

    }
}
