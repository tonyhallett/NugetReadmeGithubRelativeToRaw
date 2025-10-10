using System.Text;
using System;

namespace NugetReadmeGithubRelativeToRaw.Rewriter
{
    internal class LineBuilder
    {
        private readonly StringBuilder _sb = new StringBuilder();

        public void AppendLine(string line, bool isLast)
        {
            _sb.Append(line);
            if (!isLast)
            {
                _sb.Append(Environment.NewLine);
            }
        }

        public override string ToString() => _sb.ToString();
    }
}
