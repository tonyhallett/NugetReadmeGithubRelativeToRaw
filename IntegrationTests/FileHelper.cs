namespace IntegrationTests
{
    internal static class FileHelper {
        public static void WriteAllTextEnsureDirectory(string path, string contents)
        {
            var directory = Path.GetDirectoryName(path);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory!);
            }
            File.WriteAllText(path, contents);
        }
    }

}