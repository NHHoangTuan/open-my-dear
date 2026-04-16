using OpenMyDear.Wpf.Models;

namespace OpenMyDear.Wpf.Services;

public interface IProfileStorageService
{
    Task<IReadOnlyList<ProfileModel>> LoadAsync(CancellationToken cancellationToken = default);

    Task SaveAsync(IReadOnlyList<ProfileModel> profiles, CancellationToken cancellationToken = default);
}
