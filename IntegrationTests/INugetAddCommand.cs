namespace IntegrationTests
{
    internal interface INugetAddCommand
    {
        void AddPackageToLocalFeed(string nupkgPath, string localFeed);
    }
}