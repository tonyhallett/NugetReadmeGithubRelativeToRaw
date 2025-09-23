using System.IO;
using Microsoft.Build.Framework;

namespace NugetReadmeGithubRelativeToRaw
{
    public class ReadmeRewriterTask : Microsoft.Build.Utilities.Task
    {
        [Required]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public string BaseReadme { get; set; }      // absolute path
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        [Required]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public string OutputReadme { get; set; }    // absolute path
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        [Required]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public string RepositoryUrl { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        public string RepositoryBranch { get; set; } = "master";

        public override bool Execute()
        {
            if (!File.Exists(BaseReadme))
            {
                Log.LogError("BaseReadme file does not exist: " + BaseReadme);
                return false;
            }

            var readMeContents = File.ReadAllText(BaseReadme);
            var packageReadMe = new ReadmeRewriter().Rewrite(readMeContents, RepositoryUrl, RepositoryBranch);
            if (packageReadMe == null)
            {
                Log.LogError("Could not parse the RepositoryUrl: " + RepositoryUrl);
                return false;
            }

            File.WriteAllText(OutputReadme, packageReadMe);
            return true;
        }
    }
}
