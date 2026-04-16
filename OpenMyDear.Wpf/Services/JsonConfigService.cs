using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenMyDear.Wpf.Helpers;
using OpenMyDear.Wpf.Models;

namespace OpenMyDear.Wpf.Services;

public sealed class JsonConfigService : IConfigService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };

    public async Task<AppConfigModel> LoadAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var configPath = AppPaths.ConfigPath;
            if (!File.Exists(configPath))
            {
                return new AppConfigModel();
            }

            await using var stream = File.OpenRead(configPath);
            var config = await JsonSerializer.DeserializeAsync<AppConfigModel>(stream, JsonOptions, cancellationToken);
            return config ?? new AppConfigModel();
        }
        catch
        {
            return new AppConfigModel();
        }
    }

    public async Task SaveAsync(AppConfigModel config, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(AppPaths.DefaultAppDataDirectory);

        await using var stream = File.Create(AppPaths.ConfigPath);
        await JsonSerializer.SerializeAsync(stream, config, JsonOptions, cancellationToken);
    }
}
