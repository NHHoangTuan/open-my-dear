using System.Diagnostics;
using System.IO;
using OpenMyDear.Wpf.Models;

namespace OpenMyDear.Wpf.Services;

public sealed class ProfileLauncherService : IProfileLauncherService
{
    public async Task<ProfileRunResultModel> RunAsync(ProfileModel profile, CancellationToken cancellationToken = default)
    {
        var result = new ProfileRunResultModel
        {
            Total = profile.Items.Count
        };

        if (profile.LaunchMode == LaunchMode.Parallel)
        {
            var tasks = profile.Items.Select(item => LaunchItemAsync(item, cancellationToken));
            var launchResults = await Task.WhenAll(tasks);
            BuildResult(result, launchResults);
            return result;
        }

        var delay = TimeSpan.FromSeconds(Math.Max(0, profile.DelaySeconds));
        var sequentialResults = new List<(LaunchItemModel Item, bool Succeeded, string? ErrorMessage)>();

        foreach (var item in profile.Items)
        {
            cancellationToken.ThrowIfCancellationRequested();
            sequentialResults.Add(await LaunchItemAsync(item, cancellationToken));

            if (!ReferenceEquals(item, profile.Items.Last()))
            {
                await Task.Delay(delay, cancellationToken);
            }
        }

        BuildResult(result, sequentialResults);
        return result;
    }

    private static void BuildResult(
        ProfileRunResultModel result,
        IEnumerable<(LaunchItemModel Item, bool Succeeded, string? ErrorMessage)> launchResults)
    {
        foreach (var launchResult in launchResults)
        {
            if (launchResult.Succeeded)
            {
                result.Succeeded++;
                continue;
            }

            result.Failed++;
            result.Errors.Add(new RunErrorModel
            {
                ItemId = launchResult.Item.Id,
                ItemLabel = launchResult.Item.Label,
                Message = launchResult.ErrorMessage ?? "Unknown launch error"
            });
        }
    }

    private static Task<(LaunchItemModel Item, bool Succeeded, string? ErrorMessage)> LaunchItemAsync(
        LaunchItemModel item,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!PathExists(item))
        {
            return Task.FromResult<(LaunchItemModel Item, bool Succeeded, string? ErrorMessage)>((item, false, "Path not found"));
        }

        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = item.Path,
                UseShellExecute = true
            };

            Process.Start(startInfo);
            return Task.FromResult<(LaunchItemModel Item, bool Succeeded, string? ErrorMessage)>((item, true, null));
        }
        catch (Exception ex)
        {
            return Task.FromResult<(LaunchItemModel Item, bool Succeeded, string? ErrorMessage)>((item, false, ex.Message));
        }
    }

    private static bool PathExists(LaunchItemModel item)
    {
        return item.Type switch
        {
            ItemType.Folder => Directory.Exists(item.Path),
            ItemType.App => File.Exists(item.Path),
            ItemType.File => File.Exists(item.Path),
            _ => false
        };
    }
}
