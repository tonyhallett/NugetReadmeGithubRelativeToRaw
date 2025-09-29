using System.Text;
using System;

namespace NugetReadmeGithubRelativeToRaw.Rewriter
{
    internal class RemoveReplacer : IRemoveReplacer
    {
        private readonly IRemoveReplaceRegexesFactory _removeReplaceRegexesFactory;

        public RemoveReplacer(IRemoveReplaceRegexesFactory removeReplaceRegexesFactory)
            => _removeReplaceRegexesFactory = removeReplaceRegexesFactory;

        public string RemoveReplace(string input, RemoveReplaceSettings settings)
        {
            var removeReplaceRegexes = _removeReplaceRegexesFactory.Create(settings);
            if (!removeReplaceRegexes.Any)
            {
                return input;
            }
            // Normalize line endings so splitting is predictable
            var normalized = input.Replace("\r\n", "\n");

            // Keep empty entries (we want to preserve blank lines)
            var lines = normalized.Split('\n');

            RemoveCommentRegexes? removeCommentRegexes = settings.RemoveCommentIdentifiers != null
                ? RemoveCommentRegexes.Create(settings.RemoveCommentIdentifiers)
                : null;
            var sb = new StringBuilder();
            bool inRemoval = false;

            string replacementText = "";
            for (int i = 0; i < lines.Length; i++)
            {
                var isLast = i == lines.Length - 1;
                var line = lines[i];

                if (!inRemoval)
                {
                    var matchStartResult = removeReplaceRegexes.MatchStart(line);
                    var startMatch = matchStartResult.Match;
                    if (!startMatch.Success)
                    {
                        // No start marker -> keep whole line
                        AppendLine(line, isLast);
                    }
                    else
                    {

                        // Found a start marker on this line
                        var before = line.Substring(0, startMatch.Index);
                        if (matchStartResult.IsRemaining)
                        {
                            AppendLine(before + (matchStartResult.ReplacementText ?? ""), true);
                            break;
                        }
                        replacementText = matchStartResult.ReplacementText ?? "";
                        var afterStart = line.Substring(startMatch.Index + startMatch.Length);

                        // Maybe the end marker is on the same line (start and end on one line)
                        var endMatchInSameLine = removeReplaceRegexes.MatchEnd(afterStart);
                        if (endMatchInSameLine.Success)
                        {
                            // Keep text before the start marker + text after the end marker
                            var afterEnd = afterStart.Substring(endMatchInSameLine.Index + endMatchInSameLine.Length);
                            var merged = before + replacementText + afterEnd;
                            // Add merged only if not empty (this preserves inline non-comment snippets)
                            AppendLine(merged, isLast);
                            // remain not inRemoval
                        }
                        else
                        {
                            // Start marker without end on same line.
                            // Preserve text before the start marker (if any), start removing from now on.
                            if (!string.IsNullOrWhiteSpace(before))
                                AppendLine(before, isLast); // preserve inline "left" content

                            inRemoval = true; // drop subsequent lines until we find end marker
                        }
                    }
                }
                else
                {
                    // We are inside a removal block: look for end marker on this line
                    var endMatch = removeReplaceRegexes.MatchEnd(line);
                    if (!endMatch.Success)
                    {
                        // No end marker -> drop the whole line
                        continue;
                    }
                    else
                    {
                        // Found end marker. Preserve anything after the end marker.
                        var after = line.Substring(endMatch.Index + endMatch.Length);
                        var appendText = replacementText + after;
                        if (!string.IsNullOrWhiteSpace(appendText))
                            AppendLine(appendText, isLast);

                        inRemoval = false;
                    }
                }
            }

            // Note: sb.AppendLine used Environment.NewLine for lines, so final string will use platform line endings.
            return sb.ToString();

            void AppendLine(string line, bool isLast)
            {
                sb.Append(line);
                if (!isLast)
                    sb.Append(Environment.NewLine);
            }
        }
    }
}
