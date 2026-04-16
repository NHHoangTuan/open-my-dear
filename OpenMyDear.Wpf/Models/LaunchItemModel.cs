namespace OpenMyDear.Wpf.Models;

public sealed class LaunchItemModel
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string Label { get; set; } = "New Item";

    public string Path { get; set; } = string.Empty;

    public ItemType Type { get; set; } = ItemType.App;

    public string? OpenWith { get; set; }

    public string? OpenWithName { get; set; }

    public string? OpenWithIcon { get; set; }
}
