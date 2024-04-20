using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Randomizer.Data.ViewModels;

namespace Randomizer.App.Controls;

public partial class GameSettingsItemPanel : UserControl
{
    public GameSettingsItemPanel()
    {
        InitializeComponent();
    }

    public static readonly DependencyProperty DataProperty =
        DependencyProperty.Register(nameof(Data),
            propertyType: typeof(GenerationWindowViewModel),
            ownerType: typeof(GameSettingsItemPanel),
            typeMetadata: new PropertyMetadata());

    public GenerationWindowViewModel Data
    {
        get { return (GenerationWindowViewModel)GetValue(DataProperty); }
        set { SetValue(DataProperty, value); }
    }

    private void ResetItemsButton_OnClick(object sender, RoutedEventArgs e)
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

    private void LocationsRegionFilter_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        Data.Items.UpdateLocationOptions(Data.Items.SelectedRegion);
    }

    private void ResetAllLocationsButton_OnClick(object sender, RoutedEventArgs e)
    {
        foreach (var location in Data.Items.LocationOptions)
        {
            location.SelectedOption = location.Options.First();
        }
    }

    private void GameSettingsItemPanel_OnLoaded(object sender, RoutedEventArgs e)
    {
        DataContext = Data;
    }
}

