namespace OpenMyDear.Wpf.Models;

public sealed class ProfileRunResultModel
{
    public int Total { get; set; }

    public int Succeeded { get; set; }

    public int Failed { get; set; }

    public List<RunErrorModel> Errors { get; set; } = [];
}
