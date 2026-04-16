using System.IO;
using OpenMyDear.Wpf.Helpers;

namespace OpenMyDear.Wpf.Services;

public sealed class ProfileMigrationService : IProfileMigrationService
{
    public Task<(bool Succeeded, string? Error)> MoveProfilesAsync(string? fromStorageDirectory, string? toStorageDirectory, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var sourcePath = AppPaths.ResolveProfilesPath(fromStorageDirectory);
            var destinationPath = AppPaths.ResolveProfilesPath(toStorageDirectory);

            if (string.Equals(sourcePath, destinationPath, StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult<(bool Succeeded, string? Error)>((true, null));
            }

            var destinationDirectory = Path.GetDirectoryName(destinationPath);
            if (string.IsNullOrWhiteSpace(destinationDirectory))
            {
                return Task.FromResult<(bool Succeeded, string? Error)>((false, "Invalid destination directory."));
            }

            Directory.CreateDirectory(destinationDirectory);

            if (!File.Exists(sourcePath))
            {
                return Task.FromResult<(bool Succeeded, string? Error)>((true, null));
            }

            var backupPath = destinationPath + ".bak";
            if (File.Exists(destinationPath))
            {
                File.Copy(destinationPath, backupPath, overwrite: true);
            }

            File.Copy(sourcePath, destinationPath, overwrite: true);
            return Task.FromResult<(bool Succeeded, string? Error)>((true, null));
        }
        catch (Exception ex)
        {
            return Task.FromResult<(bool Succeeded, string? Error)>((false, ex.Message));
        }
    }
}
