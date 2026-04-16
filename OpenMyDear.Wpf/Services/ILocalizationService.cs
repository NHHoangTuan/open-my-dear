using System.ComponentModel;

namespace OpenMyDear.Wpf.Services;

public interface ILocalizationService : INotifyPropertyChanged
{
    string CurrentLanguage { get; }

    string[] SupportedLanguages { get; }

    string this[string key] { get; }

    Task SetLanguageAsync(string languageCode, CancellationToken cancellationToken = default);
}
