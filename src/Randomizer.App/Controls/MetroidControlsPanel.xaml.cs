using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using Randomizer.Data.Options;

namespace Randomizer.App.Controls;

/// <summary>
/// Interaction logic for SMControlsPanel.xaml
/// </summary>
public partial class MetroidControlsPanel : UserControl
{
    public MetroidControlsPanel()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Function to prevent mapping the same button to multiple commands
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ComboBox_ButtonMappingChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is not ComboBox comboBox) return;
        var updatedButton = (MetroidButton)comboBox.SelectedItem;
        var dropdowns = new List<ComboBox?>()
        {
            ComboBox_Shoot,
            ComboBox_Jump,
            ComboBox_Dash,
            ComboBox_ItemSelect,
            ComboBox_ItemCancel,
            ComboBox_AngleUp,
            ComboBox_AngleDown,
        };
        var duplicateDropdown = dropdowns.FirstOrDefault(x => x != null && (MetroidButton)x.SelectedItem == updatedButton && x != comboBox);
        if (duplicateDropdown == null) return;
        var selectedButtons = dropdowns.Select(x => (MetroidButton)x!.SelectedItem).Distinct().ToList();
        var missingButton = Enum.GetValues<MetroidButton>().First(x => !selectedButtons.Contains(x));
        duplicateDropdown.SelectedItem = missingButton;
    }

    private void SetLabels()
    {
        var options = DataContext as MetroidControlOptions;

        if (RunButtonLabel == null) return;

        RunButtonLabel.Text = options?.RunButtonBehavior switch
        {
            RunButtonBehavior.AutoRun => "Walk button",
            _ => "Run button"
        };

        ItemCancelLabel.Text = options?.ItemCancelBehavior switch
        {
            ItemCancelBehavior.HoldSupersOnly => "Hold to equip",
            ItemCancelBehavior.Hold => "Hold to equip",
            ItemCancelBehavior.Toggle => "Quick item toggle",
            _ => "Item Cancel button"
        };

        AimUpLabel.Text = options?.AimButtonBehavior switch
        {
            AimButtonBehavior.UnifiedAim => "Aim button",
            _ => "Angle up button"
        };

        AimDownLabel.Text = options?.AimButtonBehavior switch
        {
            AimButtonBehavior.UnifiedAim => "Quick Morph button",
            _ => "Angle down button"
        };

    }

    private void ComboBox_OptionSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        SetLabels();
    }

    private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        SetLabels();
    }
}
