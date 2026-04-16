using Microsoft.Win32;

namespace OpenMyDear.Wpf.Services;

public sealed class AutostartService : IAutostartService
{
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ValueName = "OpenMyDear";

    public Task<(bool Succeeded, string? Error)> SetEnabledAsync(bool enabled, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            using var runKey = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: true) ?? Registry.CurrentUser.CreateSubKey(RunKeyPath, writable: true);
            if (runKey is null)
            {
                return Task.FromResult<(bool Succeeded, string? Error)>((false, "Unable to open startup registry key."));
            }

            if (enabled)
            {
                var executablePath = Environment.ProcessPath;
                if (string.IsNullOrWhiteSpace(executablePath))
                {
                    return Task.FromResult<(bool Succeeded, string? Error)>((false, "Unable to resolve executable path."));
                }

                runKey.SetValue(ValueName, $"\"{executablePath}\"");
            }
            else
            {
                runKey.DeleteValue(ValueName, throwOnMissingValue: false);
            }

            return Task.FromResult<(bool Succeeded, string? Error)>((true, null));
        }
        catch (Exception ex)
        {
            return Task.FromResult<(bool Succeeded, string? Error)>((false, ex.Message));
        }
    }
}
