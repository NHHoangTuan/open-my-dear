using OpenMyDear.Wpf.Models;

namespace OpenMyDear.Wpf.Services;

public interface IProfileLauncherService
{
    Task<ProfileRunResultModel> RunAsync(ProfileModel profile, CancellationToken cancellationToken = default);
}
