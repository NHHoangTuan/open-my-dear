using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;
using OpenMyDear.Wpf.Models;

namespace OpenMyDear.Wpf.Services;

public sealed class JsonProfileStorageService : IProfileStorageService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };

    private readonly string _filePath;

    public JsonProfileStorageService()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var folder = Path.Combine(appData, "OpenMyDear.Wpf");
        _filePath = Path.Combine(folder, "profiles.json");
    }

    public async Task<IReadOnlyList<ProfileModel>> LoadAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (!File.Exists(_filePath))
            {
                return [];
            }

            await using var stream = File.OpenRead(_filePath);
            var profiles = await JsonSerializer.DeserializeAsync<List<ProfileModel>>(stream, JsonOptions, cancellationToken);
            return profiles ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task SaveAsync(IReadOnlyList<ProfileModel> profiles, CancellationToken cancellationToken = default)
    {
        var directoryPath = Path.GetDirectoryName(_filePath);
        if (string.IsNullOrWhiteSpace(directoryPath))
        {
            throw new InvalidOperationException("Unable to determine profiles storage directory.");
        }

        Directory.CreateDirectory(directoryPath);

        await using var stream = File.Create(_filePath);
        await JsonSerializer.SerializeAsync(stream, profiles, JsonOptions, cancellationToken);
    }
}
