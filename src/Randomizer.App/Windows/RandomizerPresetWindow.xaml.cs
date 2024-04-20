using System.Windows;

namespace Randomizer.App.Windows;

public partial class RandomizerPresetWindow : Window
{
    public RandomizerPresetWindow()
    {
        InitializeComponent();
    }

    public string PresetName = "";

    private void SavePresetButton_OnClick(object sender, RoutedEventArgs e)
    {
        PresetName = PresetNameTextBox.Text;
        DialogResult = true;
        Close();
    }
}

