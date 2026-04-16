using OpenMyDear.Wpf.Models;

namespace OpenMyDear.Wpf.Services;

public interface IConfigService
{
    Task<AppConfigModel> LoadAsync(CancellationToken cancellationToken = default);

    Task SaveAsync(AppConfigModel config, CancellationToken cancellationToken = default);
}
