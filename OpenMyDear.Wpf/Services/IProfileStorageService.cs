using OpenMyDear.Wpf.Models;

namespace OpenMyDear.Wpf.Services;

public interface IProfileStorageService
{
    Task<IReadOnlyList<ProfileModel>> LoadAsync(string? storageDirectory, CancellationToken cancellationToken = default);

    Task SaveAsync(IReadOnlyList<ProfileModel> profiles, string? storageDirectory, CancellationToken cancellationToken = default);
}
