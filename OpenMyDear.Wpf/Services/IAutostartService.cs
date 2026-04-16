namespace OpenMyDear.Wpf.Services;

public interface IAutostartService
{
    Task<(bool Succeeded, string? Error)> SetEnabledAsync(bool enabled, CancellationToken cancellationToken = default);
}
