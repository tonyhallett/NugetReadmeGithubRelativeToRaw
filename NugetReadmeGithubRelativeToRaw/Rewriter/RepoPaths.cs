using System;
using System.Linq;

namespace NugetReadmeGithubRelativeToRaw.Rewriter
{
    internal class RepoPaths
    {
        public string ImageBasePath { get; }

        public string LinkBasePath { get; }
        
        public string ReadmeRelativePath { get; }
        
        private RepoPaths(string imageBasePath,string linkBasePath, string readmeRelativePath)
        {
            ImageBasePath = imageBasePath;
            LinkBasePath = linkBasePath;
            ReadmeRelativePath = readmeRelativePath;
        }

        public static RepoPaths? Create(string repoUrl, string @ref, string readmeRelativePath)
        {
            repoUrl = GetRepoUrl(repoUrl);

            if (repoUrl.StartsWith("https://github.com/", StringComparison.OrdinalIgnoreCase))
            {
                var parts = repoUrl.Substring("https://github.com/".Length).Split('/');
                if (parts.Length >= 2)
                {
                    var ownerRepo = $"{parts[0]}/{parts[1]}";
                    var imageBasePath = $"https://raw.githubusercontent.com/{ownerRepo}/{@ref}";
                    var linkBasePath = $"https://github.com/{ownerRepo}/blob/{@ref}";
                    return new RepoPaths(imageBasePath, linkBasePath, readmeRelativePath);
                }
            }
            /*
                https://gitlab.com/{namespace}/{project}/-/raw/{ref}/{path}
                https://gitlab.com/{namespace}/{project}/-/blob/{ref}/{path}
            */
            if(repoUrl.StartsWith("https://gitlab.com/", StringComparison.OrdinalIgnoreCase))
            {
                var parts = repoUrl.Substring("https://gitlab.com/".Length).Split('/');
                if(parts.Length >= 2)
                {
                    var project = parts[parts.Length - 1];
                    var namespaceParts = parts.Take(parts.Length - 1);
                    var namespaceProject = string.Join("/", namespaceParts) + "/" + project;
                    return new RepoPaths(GetGitLabBasePath(namespaceProject, true, @ref), GetGitLabBasePath(namespaceProject, false, @ref), readmeRelativePath);
                }
            }
            return null;
        }

        private static string GetGitLabBasePath(string namespaceProject, bool isImage, string @ref)
        {
            var rawOrBlob = isImage ? "raw" : "blob";
            return $"https://gitlab.com/{namespaceProject}/-/{rawOrBlob}/{@ref}";
        }

        private static string GetRepoUrl(string repoUrl)
        {
            repoUrl = repoUrl.TrimEnd('/');
            if (repoUrl.EndsWith(".git", StringComparison.OrdinalIgnoreCase))
            {
                repoUrl = repoUrl.Substring(0, repoUrl.Length - 4);
            }

            return repoUrl;
        }
    }
}
