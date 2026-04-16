namespace OpenMyDear.Wpf.Services;

public interface IProfileMigrationService
{
    Task<(bool Succeeded, string? Error)> MoveProfilesAsync(string? fromStorageDirectory, string? toStorageDirectory, CancellationToken cancellationToken = default);
}
