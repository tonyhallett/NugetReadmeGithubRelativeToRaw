using NugetReadmeGithubRelativeToRaw;

namespace UnitTests
{
    internal class CollectingAddError : IAddError
    {
        public List<string> Errors { get; } = new List<string>();

        public void AddError(string message)
        {
            Errors.Add(message);
        }

        public string Single() => Errors.Single();
    }
}
