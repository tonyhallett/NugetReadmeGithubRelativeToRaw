using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace NugetReadmeGithubRelativeToRaw.Rewriter
{
    internal class RemoveReplaceRegexes : IRemoveReplaceRegexes
    {
        private readonly RemoveCommentRegexes? _removeCommentRegexes;
        private readonly List<RegexRemovalOrReplacement> regexRemovalOrReplacements;
        private bool matchedRemoveCommentRegexes = false;
        private RegexRemovalOrReplacement? matchedRegexRemovalOrReplacement;


        public RemoveReplaceRegexes(
            List<RegexRemovalOrReplacement> regexRemovalOrReplacements,
            RemoveCommentRegexes? removeCommentRegexes)
        {
            this.regexRemovalOrReplacements = regexRemovalOrReplacements;
            _removeCommentRegexes = removeCommentRegexes;
        }

        public bool Any => (_removeCommentRegexes != null) || (regexRemovalOrReplacements.Count > 0);

        public MatchStartResult MatchStart(string text)
        {
            Match match = Match.Empty;
            if (_removeCommentRegexes != null)
            {
                match = _removeCommentRegexes.StartRegex.Match(text);
                if (match.Success)
                {
                    matchedRemoveCommentRegexes = true;
                    return new MatchStartResult(match, false, null);
                }
            }
            
            foreach (var regexRemovalOrReplacement in regexRemovalOrReplacements)
            {
                match = regexRemovalOrReplacement.StartRegex.Match(text);
                if (match.Success)
                {
                    if (regexRemovalOrReplacement.EndRegex != null)
                    {
                        matchedRegexRemovalOrReplacement = regexRemovalOrReplacement;
                    }

                    return new MatchStartResult(match, regexRemovalOrReplacement.EndRegex == null, regexRemovalOrReplacement.ReplacementText);
                }
            }

            return new MatchStartResult(match);
        }

        public Match MatchEnd(string text)
        {
            Match match = Match.Empty;
            if (matchedRemoveCommentRegexes)
            {
                match = _removeCommentRegexes!.EndRegex.Match(text);
                if(match.Success)
                {
                    matchedRemoveCommentRegexes = false;
                }
                return match;
            }

            match = matchedRegexRemovalOrReplacement!.EndRegex!.Match(text);
            if (match.Success)
            {
                matchedRegexRemovalOrReplacement = null;
                //regexRemovalOrReplacements.Remove(matchedRegexRemovalOrReplacement!);
            }

            return match;
        }
    }
}
