namespace OpenMyDear.Wpf.Services;

public interface IThemeService
{
    string CurrentTheme { get; }

    string[] SupportedThemes { get; }

    void ApplyTheme(string theme);
}
