namespace OpenMyDear.Wpf.Models;

public sealed class AppConfigModel
{
    public string? StorageDirectory { get; set; }

    public bool AlwaysOnTop { get; set; }

    public bool AutostartEnabled { get; set; }

    public string Language { get; set; } = "en";
}
