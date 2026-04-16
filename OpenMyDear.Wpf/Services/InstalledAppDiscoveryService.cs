using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using OpenMyDear.Wpf.Models;

namespace OpenMyDear.Wpf.Services;

public sealed class InstalledAppDiscoveryService : IInstalledAppDiscoveryService
{
    private const string AppPathsKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths";

    public Task<IReadOnlyList<InstalledAppModel>> GetInstalledAppsAsync(CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            var map = new Dictionary<string, InstalledAppModel>(StringComparer.OrdinalIgnoreCase);

            ReadRegistryAppPaths(RegistryHive.LocalMachine, map, cancellationToken);
            ReadRegistryAppPaths(RegistryHive.CurrentUser, map, cancellationToken);
            ReadStartMenuShortcuts(map, cancellationToken);

            var results = map.Values
                .OrderBy(app => app.Name, StringComparer.CurrentCultureIgnoreCase)
                .ThenBy(app => app.ExecutablePath, StringComparer.OrdinalIgnoreCase)
                .ToList();

            return (IReadOnlyList<InstalledAppModel>)results;
        }, cancellationToken);
    }

    private static void ReadRegistryAppPaths(
        RegistryHive hive,
        IDictionary<string, InstalledAppModel> map,
        CancellationToken cancellationToken)
    {
        try
        {
            using var root = RegistryKey.OpenBaseKey(hive, RegistryView.Default);
            using var appPaths = root.OpenSubKey(AppPathsKey);
            if (appPaths is null)
            {
                return;
            }

            foreach (var subKeyName in appPaths.GetSubKeyNames())
            {
                cancellationToken.ThrowIfCancellationRequested();

                using var subKey = appPaths.OpenSubKey(subKeyName);
                var value = subKey?.GetValue(null) as string;
                if (string.IsNullOrWhiteSpace(value))
                {
                    continue;
                }

                var executablePath = NormalizeExecutablePath(value);
                if (string.IsNullOrWhiteSpace(executablePath) || !File.Exists(executablePath))
                {
                    continue;
                }

                var appName = Path.GetFileNameWithoutExtension(subKeyName);
                map[executablePath] = new InstalledAppModel
                {
                    Name = string.IsNullOrWhiteSpace(appName) ? Path.GetFileNameWithoutExtension(executablePath) : appName,
                    ExecutablePath = executablePath
                };
            }
        }
        catch
        {
        }
    }

    private static void ReadStartMenuShortcuts(
        IDictionary<string, InstalledAppModel> map,
        CancellationToken cancellationToken)
    {
        var directories = new[]
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu), "Programs"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Programs")
        };

        foreach (var directory in directories)
        {
            try
            {
                if (!Directory.Exists(directory))
                {
                    continue;
                }

                foreach (var shortcutPath in Directory.EnumerateFiles(directory, "*.lnk", SearchOption.AllDirectories))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var executablePath = ResolveShortcutTarget(shortcutPath);
                    if (string.IsNullOrWhiteSpace(executablePath) || !File.Exists(executablePath))
                    {
                        continue;
                    }

                    if (!string.Equals(Path.GetExtension(executablePath), ".exe", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    var name = Path.GetFileNameWithoutExtension(shortcutPath);
                    map[executablePath] = new InstalledAppModel
                    {
                        Name = string.IsNullOrWhiteSpace(name) ? Path.GetFileNameWithoutExtension(executablePath) : name,
                        ExecutablePath = executablePath
                    };
                }
            }
            catch
            {
            }
        }
    }

    private static string? ResolveShortcutTarget(string shortcutPath)
    {
        object? shellObject = null;
        object? shortcutObject = null;

        try
        {
            var shellType = Type.GetTypeFromProgID("WScript.Shell");
            if (shellType is null)
            {
                return null;
            }

            shellObject = Activator.CreateInstance(shellType);
            if (shellObject is null)
            {
                return null;
            }

            dynamic shell = shellObject;
            shortcutObject = shell.CreateShortcut(shortcutPath);
            dynamic shortcut = shortcutObject;
            var targetPath = shortcut.TargetPath as string;
            return NormalizeExecutablePath(targetPath);
        }
        catch
        {
            return null;
        }
        finally
        {
            ReleaseComObject(shortcutObject);
            ReleaseComObject(shellObject);
        }
    }

    private static string? NormalizeExecutablePath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        var cleaned = path.Trim().Trim('"');
        return cleaned;
    }

    private static void ReleaseComObject(object? comObject)
    {
        try
        {
            if (comObject is not null && Marshal.IsComObject(comObject))
            {
                Marshal.FinalReleaseComObject(comObject);
            }
        }
        catch
        {
        }
    }
}
