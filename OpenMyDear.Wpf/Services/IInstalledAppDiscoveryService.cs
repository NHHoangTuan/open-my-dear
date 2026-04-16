using OpenMyDear.Wpf.Models;

namespace OpenMyDear.Wpf.Services;

public interface IInstalledAppDiscoveryService
{
    Task<IReadOnlyList<InstalledAppModel>> GetInstalledAppsAsync(CancellationToken cancellationToken = default);
}
