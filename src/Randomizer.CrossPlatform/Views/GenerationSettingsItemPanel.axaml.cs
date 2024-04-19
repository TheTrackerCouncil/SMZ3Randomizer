using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Randomizer.Data.ViewModels;

namespace Randomizer.CrossPlatform.Views;

public partial class GenerationSettingsItemPanel : UserControl
{
    public GenerationSettingsItemPanel()
    {
        InitializeComponent();
    }

    public static readonly StyledProperty<GenerationWindowViewModel> DataProperty = AvaloniaProperty.Register<GenerationSettingsItemPanel, GenerationWindowViewModel>(
        nameof(Data));

    public GenerationWindowViewModel Data
    {
        get => GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    private void ResetItemsButton_OnClick(object? sender, RoutedEventArgs e)
    {
        foreach (var data in Data.Items.MetroidItemOptions)
        {
            data.SelectedOption = data.Options.First();
        }

        foreach (var data in Data.Items.ZeldaItemOptions)
        {
            data.SelectedOption = data.Options.First();
        }
    }

    private void Control_OnLoaded(object? sender, RoutedEventArgs e)
    {
        DataContext = Data;
    }

    private void ResetAllLocationsButton_OnClick(object? sender, RoutedEventArgs e)
    {
        foreach (var location in Data.Items.LocationOptions)
        {
            location.SelectedOption = location.Options.First();
        }
    }

    private void LocationsRegionFilter_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        Data.Items.UpdateLocationOptions(Data.Items.SelectedRegion);
    }
}

