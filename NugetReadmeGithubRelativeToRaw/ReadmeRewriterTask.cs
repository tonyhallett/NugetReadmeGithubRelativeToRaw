using System.IO;
using Microsoft.Build.Framework;
using NugetReadmeGithubRelativeToRaw.Rewriter;

namespace NugetReadmeGithubRelativeToRaw
{
    public class ReadmeRewriterTask : Microsoft.Build.Utilities.Task
    {
        [Required]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public string ProjectDirectoryPath { get; set; }      // absolute path
        [Required]
        public string ReadmeRelativePath { get; set; }      // absolute path
        [Required]
        public string OutputReadme { get; set; }    // absolute path
        [Required]
        public string RepositoryUrl { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        public string? RepositoryBranch { get; set; }

        public override bool Execute()
        {
            var readmePath = Path.Combine(ProjectDirectoryPath, ReadmeRelativePath);
            if (!File.Exists(readmePath))
            {
                Log.LogError("BaseReadme file does not exist: " + readmePath);
                return false;
            }

            var readMeContents = File.ReadAllText(readmePath);
            var readmeRewriterResult = new ReadmeRewriter().Rewrite(readMeContents, ReadmeRelativePath, RepositoryUrl, RepositoryBranch);
            if (readmeRewriterResult == null)
            {
                Log.LogError("Could not parse the RepositoryUrl: " + RepositoryUrl);
                return false;
            }

            foreach(var x in readmeRewriterResult.UnsupportedImageDomains)
            {
                Log.LogWarning("Unsupported image domain found in README: " + x);
            }

            File.WriteAllText(OutputReadme, readmeRewriterResult.RewrittenReadme);
            return true;
        }
    }
}
