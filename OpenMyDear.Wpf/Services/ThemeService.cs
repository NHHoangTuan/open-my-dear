using Microsoft.Win32;

namespace OpenMyDear.Wpf.Services;

public sealed class ThemeService : IThemeService
{
    private const string LightThemeDictionary = "Resources/Theme.Light.xaml";
    private const string DarkThemeDictionary = "Resources/Theme.Dark.xaml";

    public string CurrentTheme { get; private set; } = "system";

    public string[] SupportedThemes { get; } = ["system", "light", "dark"];

    public void ApplyTheme(string theme)
    {
        var normalized = NormalizeTheme(theme);
        var resolved = normalized == "system" ? ResolveSystemTheme() : normalized;

        var application = System.Windows.Application.Current;
        if (application is null)
        {
            CurrentTheme = normalized;
            return;
        }

        var dictionaries = application.Resources.MergedDictionaries;
        for (var i = dictionaries.Count - 1; i >= 0; i--)
        {
            var source = dictionaries[i].Source?.OriginalString;
            if (string.IsNullOrWhiteSpace(source))
            {
                continue;
            }

            if (source.Contains("Theme.Light.xaml", StringComparison.OrdinalIgnoreCase)
                || source.Contains("Theme.Dark.xaml", StringComparison.OrdinalIgnoreCase))
            {
                dictionaries.RemoveAt(i);
            }
        }

        var themePath = resolved == "dark" ? DarkThemeDictionary : LightThemeDictionary;
        dictionaries.Insert(0, new System.Windows.ResourceDictionary
        {
            Source = new Uri(themePath, UriKind.Relative)
        });

        CurrentTheme = normalized;
    }

    private string NormalizeTheme(string? theme)
    {
        if (string.IsNullOrWhiteSpace(theme))
        {
            return "system";
        }

        var normalized = theme.Trim().ToLowerInvariant();
        return SupportedThemes.Contains(normalized) ? normalized : "system";
    }

    private static string ResolveSystemTheme()
    {
        try
        {
            using var personalize = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            var useLightTheme = personalize?.GetValue("AppsUseLightTheme");
            if (useLightTheme is int value)
            {
                return value == 0 ? "dark" : "light";
            }
        }
        catch
        {
        }

        return "light";
    }
}
