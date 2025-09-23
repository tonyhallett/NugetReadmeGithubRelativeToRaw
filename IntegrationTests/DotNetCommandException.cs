namespace IntegrationTests
{
    public class DotNetCommandException : Exception
    {
        public DotNetCommandException(string command, string standardOutput, string standardError, int exitCode) : base(GetMessage(command, standardOutput, standardError, exitCode)) {
            Command = command;
            StandardOutput = standardOutput;
            StandardError = standardError;
            ExitCode = exitCode;
        }

        public string Command { get; }

        public string StandardOutput { get; }
        
        public string StandardError { get; }
        
        public int ExitCode { get; }

        private static string GetMessage(string command, string standardOutput, string standardError, int exitCode)
        {
            return $"Command '{command}' failed with exit code {exitCode}.\nStandard Output: {standardOutput}\nStandard Error: {standardError}";
        }
    }
}