using System.IO;

namespace OpenMyDear.Wpf.Helpers;

public static class AppPaths
{
    public static string DefaultAppDataDirectory => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "OpenMyDear.Wpf");

    public static string ConfigPath => Path.Combine(DefaultAppDataDirectory, "config.json");

    public static string ResolveStorageDirectory(string? configuredDirectory)
    {
        return string.IsNullOrWhiteSpace(configuredDirectory)
            ? DefaultAppDataDirectory
            : configuredDirectory;
    }

    public static string ResolveProfilesPath(string? configuredDirectory)
    {
        return Path.Combine(ResolveStorageDirectory(configuredDirectory), "profiles.json");
    }
}
