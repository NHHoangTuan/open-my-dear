using CommunityToolkit.Mvvm.ComponentModel;
using OpenMyDear.Wpf.Models;

namespace OpenMyDear.Wpf.ViewModels;

public partial class LaunchItemViewModel : ObservableObject
{
    [ObservableProperty]
    private string _id;

    [ObservableProperty]
    private string _label;

    [ObservableProperty]
    private string _path;

    [ObservableProperty]
    private ItemType _type;

    public LaunchItemViewModel()
    {
        _id = Guid.NewGuid().ToString();
        _label = "New Item";
        _path = string.Empty;
        _type = ItemType.App;
    }

    public LaunchItemViewModel(LaunchItemModel model)
    {
        _id = string.IsNullOrWhiteSpace(model.Id) ? Guid.NewGuid().ToString() : model.Id;
        _label = model.Label;
        _path = model.Path;
        _type = model.Type;
    }

    public LaunchItemModel ToModel()
    {
        return new LaunchItemModel
        {
            Id = Id,
            Label = Label,
            Path = Path,
            Type = Type
        };
    }
}
