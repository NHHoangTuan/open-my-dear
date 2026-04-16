using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;

namespace OpenMyDear.Wpf.Services;

public sealed class JsonLocalizationService : ObservableObject, ILocalizationService
{
    private readonly Dictionary<string, string> _strings = new(StringComparer.OrdinalIgnoreCase);

    private string _currentLanguage = "en";

    public string CurrentLanguage => _currentLanguage;

    public string[] SupportedLanguages { get; } = ["en", "vi"];

    public string this[string key] => _strings.TryGetValue(key, out var value) ? value : key;

    public async Task SetLanguageAsync(string languageCode, CancellationToken cancellationToken = default)
    {
        var normalized = SupportedLanguages.Contains(languageCode, StringComparer.OrdinalIgnoreCase)
            ? languageCode.ToLowerInvariant()
            : "en";

        var strings = await LoadLanguageStringsAsync(normalized, cancellationToken);
        if (strings.Count == 0 && normalized != "en")
        {
            strings = await LoadLanguageStringsAsync("en", cancellationToken);
            normalized = "en";
        }

        _strings.Clear();
        foreach (var pair in strings)
        {
            _strings[pair.Key] = pair.Value;
        }

        _currentLanguage = normalized;
        OnPropertyChanged(nameof(CurrentLanguage));
        OnPropertyChanged("Item[]");
    }

    private static async Task<Dictionary<string, string>> LoadLanguageStringsAsync(string languageCode, CancellationToken cancellationToken)
    {
        try
        {
            var filePath = Path.Combine(AppContext.BaseDirectory, "Resources", "Localization", $"{languageCode}.json");
            if (!File.Exists(filePath))
            {
                return [];
            }

            await using var stream = File.OpenRead(filePath);
            var result = await JsonSerializer.DeserializeAsync<Dictionary<string, string>>(stream, cancellationToken: cancellationToken);
            return result ?? [];
        }
        catch
        {
            return [];
        }
    }
}
