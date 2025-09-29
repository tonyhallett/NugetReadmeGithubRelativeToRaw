using System.Text.RegularExpressions;

namespace NugetReadmeGithubRelativeToRaw.Rewriter
{
    internal interface IRemoveReplaceRegexes
    {
        bool Any { get; }

        MatchStartResult MatchStart(string line);
        Match MatchEnd(string afterStart);
    }
}
