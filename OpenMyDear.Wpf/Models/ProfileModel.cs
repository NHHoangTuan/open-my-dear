namespace OpenMyDear.Wpf.Models;

public sealed class ProfileModel
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string Name { get; set; } = "New Profile";

    public List<LaunchItemModel> Items { get; set; } = [];

    public LaunchMode LaunchMode { get; set; } = LaunchMode.Parallel;

    public double DelaySeconds { get; set; } = 2.0;
}
