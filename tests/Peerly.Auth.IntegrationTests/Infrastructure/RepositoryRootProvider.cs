using System;
using System.IO;

namespace Peerly.Auth.IntegrationTests.Infrastructure;

internal static class RepositoryRootProvider
{
    private static readonly Lazy<string> s_repositoryRoot = new(FindRepositoryRoot);

    public static string GetRepositoryRoot() => s_repositoryRoot.Value;

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Peerly.Auth.sln")))
                return directory.FullName;

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Unable to locate repository root containing Peerly.Auth.sln.");
    }
}
