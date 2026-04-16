using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;
using OpenMyDear.Wpf.Helpers;
using OpenMyDear.Wpf.Models;

namespace OpenMyDear.Wpf.Services;

public sealed class JsonProfileStorageService : IProfileStorageService
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

    public async Task<IReadOnlyList<ProfileModel>> LoadAsync(string? storageDirectory, CancellationToken cancellationToken = default)
    {
        var filePath = AppPaths.ResolveProfilesPath(storageDirectory);

        try
        {
            if (!File.Exists(filePath))
            {
                return [];
            }

            await using var stream = File.OpenRead(filePath);
            var profiles = await JsonSerializer.DeserializeAsync<List<ProfileModel>>(stream, JsonOptions, cancellationToken);
            return profiles ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task SaveAsync(IReadOnlyList<ProfileModel> profiles, string? storageDirectory, CancellationToken cancellationToken = default)
    {
        var filePath = AppPaths.ResolveProfilesPath(storageDirectory);
        var directoryPath = Path.GetDirectoryName(filePath);
        if (string.IsNullOrWhiteSpace(directoryPath))
        {
            throw new InvalidOperationException("Unable to determine profiles storage directory.");
        }

        Directory.CreateDirectory(directoryPath);

        await using var stream = File.Create(filePath);
        await JsonSerializer.SerializeAsync(stream, profiles, JsonOptions, cancellationToken);
    }
}
