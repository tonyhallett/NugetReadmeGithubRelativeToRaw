namespace NugetReadmeGithubRelativeToRaw.Rewriter
{
    internal class RemoveReplacer : IRemoveReplacer
    {
        private readonly IRemoveReplaceRegexesFactory _removeReplaceRegexesFactory;

        public RemoveReplacer(IRemoveReplaceRegexesFactory removeReplaceRegexesFactory)
            => _removeReplaceRegexesFactory = removeReplaceRegexesFactory;

        private string[] GetLines(string input)
        {
            // Normalize line endings so splitting is predictable
            var normalized = input.Replace("\r\n", "\n");
            // Keep empty entries (we want to preserve blank lines)
            return normalized.Split('\n');
        }

        public string RemoveReplace(string input, RemoveReplaceSettings settings)
        {
            var removeReplaceRegexes = _removeReplaceRegexesFactory.Create(settings);
            if (!removeReplaceRegexes.Any)
            {
                return input;
            }

            var lineBuilder = new LineBuilder();
            bool inRemovalReplacement = false;
            string replacementText = "";
            var lines = GetLines(input);

            for (int i = 0; i < lines.Length; i++)
            {
                var isLast = i == lines.Length - 1;
                var line = lines[i];

                if (!inRemovalReplacement)
                {
                    var matchStartResult = removeReplaceRegexes.MatchStart(line);
                    var startMatch = matchStartResult.Match;
                    if (!startMatch.Success)
                    {
                        // No start marker -> keep whole line
                        lineBuilder.AppendLine(line, isLast);
                        continue;
                    }

                    replacementText = matchStartResult.ReplacementText ?? "";
                    var before = startMatch.Before(line);
                    if (matchStartResult.IsRemaining)
                    {
                        lineBuilder.AppendLine(before + replacementText, true);
                        break;
                    }

                    var afterStart = startMatch.After(line);

                    // Maybe the end marker is on the same line (start and end on one line)
                    var endMatchInSameLine = removeReplaceRegexes.MatchEnd(afterStart);
                    if (endMatchInSameLine.Success)
                    {
                        // Keep text before the start marker + text after the end marker
                        var afterEnd = endMatchInSameLine.After(afterStart);
                        var merged = before + replacementText + afterEnd;
                        // Add merged only if not empty (this preserves inline non-comment snippets)
                        lineBuilder.AppendLine(merged, isLast);
                    }
                    else
                    {
                        // Start marker without end on same line.
                        // Preserve text before the start marker (if any), start removing from now on.
                        if (!string.IsNullOrWhiteSpace(before))
                        {
                            lineBuilder.AppendLine(before, isLast); // preserve inline "left" content
                        }

                        inRemovalReplacement = true;
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

                    // Found end marker. Preserve anything after the end marker.
                    var after = endMatch.After(line);
                    var appendText = replacementText + after;
                    if (!string.IsNullOrWhiteSpace(appendText))
                    {
                        lineBuilder.AppendLine(appendText, isLast);
                    }

                    inRemovalReplacement = false;
                    
                }
            }

            return lineBuilder.ToString();
        }
    }
}
