using Microsoft.Build.Framework;
using NugetReadmeGithubRelativeToRaw.Rewriter;

namespace NugetReadmeGithubRelativeToRaw
{
    public class ReadmeRewriterTask : Microsoft.Build.Utilities.Task
    {
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

        internal IIOHelper IOHelper { get; set; } = new IOHelper();

        internal IReadmeRewriter ReadmeRewriter { get; set; } = new ReadmeRewriter();

        public override bool Execute()
        {
            var readmeRelativePath = ReadmeRelativePath ?? "readme.md";
            var readmePath = IOHelper.CombinePaths(ProjectDirectoryPath, readmeRelativePath);
            if (!IOHelper.FileExists(readmePath))
            {
                Log.LogError("Readme file does not exist: " + readmePath);
            }
            else
            {
                Rewrite(readmePath, readmeRelativePath);
            }
            
            return !Log.HasLoggedErrors;
        }

        private void Rewrite(string readmePath, string readmeRelativePath)
        {
            var readMeContents = IOHelper.ReadAllText(readmePath);
            var readmeRewriterResult = ReadmeRewriter.Rewrite(readMeContents, readmeRelativePath, RepositoryUrl, RepositoryBranch, GetRewriteTagsOptions());
            if (readmeRewriterResult != null)
            {
                ProcessReadmeWriteResult(readmeRewriterResult);
            }
            else
            {
                Log.LogError("Could not parse the RepositoryUrl: " + RepositoryUrl);
            }
        }

        private RewriteTagsOptions GetRewriteTagsOptions()
        {
            var options = Rewriter.RewriteTagsOptions.All;
            if(RewriteTagsOptions != null)
            {
                try
                {
                    options = RewriteTagsOptionsParser.Parse(RewriteTagsOptions);
                }
                catch
                {
                    Log.LogWarning("Could not parse the RewriteTagsOptions: " + RewriteTagsOptions + ". Using the default: " + options);
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
                    Log.LogError("Unsupported image domain found in README: " + unsupportedImageDomain);
                }
            }
            else
            {
                IOHelper.WriteAllText(OutputReadme, readmeRewriterResult.RewrittenReadme);
            }
        }
    }
}
