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
        var sequentialResults = new List<LaunchOutcome>();

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

    public async Task<ProfileRunResultModel> RunItemAsync(LaunchItemModel item, CancellationToken cancellationToken = default)
    {
        var result = new ProfileRunResultModel
        {
            Total = 1
        };

        var launchResult = await LaunchItemAsync(item, cancellationToken);
        BuildResult(result, [launchResult]);
        return result;
    }

    private static void BuildResult(
        ProfileRunResultModel result,
        IEnumerable<LaunchOutcome> launchResults)
    {
        foreach (var launchResult in launchResults)
        {
            if (!string.IsNullOrWhiteSpace(launchResult.WarningMessage))
            {
                result.Warnings.Add($"{launchResult.Item.Label}: {launchResult.WarningMessage}");
            }

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

    private static Task<LaunchOutcome> LaunchItemAsync(
        LaunchItemModel item,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!PathExists(item))
        {
            return Task.FromResult(new LaunchOutcome(item, false, "Path not found", null));
        }

        var warningMessage = default(string);

        try
        {
            if (!string.IsNullOrWhiteSpace(item.OpenWith))
            {
                if (File.Exists(item.OpenWith))
                {
                    var openWithStartInfo = new ProcessStartInfo
                    {
                        FileName = item.OpenWith,
                        Arguments = $"\"{item.Path}\"",
                        UseShellExecute = true
                    };

                    Process.Start(openWithStartInfo);
                    return Task.FromResult(new LaunchOutcome(item, true, null, null));
                }

                warningMessage = "OpenWith path invalid. Used default open.";
            }

            var defaultStartInfo = new ProcessStartInfo
            {
                FileName = item.Path,
                UseShellExecute = true
            };

            Process.Start(defaultStartInfo);
            return Task.FromResult(new LaunchOutcome(item, true, null, warningMessage));
        }
        catch (Exception ex)
        {
            return Task.FromResult(new LaunchOutcome(item, false, ex.Message, warningMessage));
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

    private sealed record LaunchOutcome(LaunchItemModel Item, bool Succeeded, string? ErrorMessage, string? WarningMessage);
}
