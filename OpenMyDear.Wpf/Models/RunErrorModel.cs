namespace OpenMyDear.Wpf.Models;

public sealed class RunErrorModel
{
    public required string ItemId { get; init; }

    public required string ItemLabel { get; init; }

    public required string Message { get; init; }

    public override string ToString() => $"{ItemLabel}: {Message}";
}
