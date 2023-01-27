using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Randomizer.Data.Options;

namespace Randomizer.App.Controls
{
    /// <summary>
    /// Interaction logic for ItemSettingsPanel.xaml
    /// </summary>
    public partial class ItemSettingsPanel : UserControl
    {
        public ItemSettingsPanel()
        {
            InitializeComponent();
        }

        private SeedOptions _seedOptions => (SeedOptions)DataContext;
        private List<ComboBox> _comboBoxes = new();

        public void InitDropdowns()
        {
            foreach (var itemOptions in ItemSettingOptions.GetOptions().OrderBy(x => x.IsMetroid).ThenBy(x => x.Item))
            {
                AddItemComboBox(itemOptions);
            }
        }

        public void AddItemComboBox(ItemSettingOptions itemOptions)
        {
            var selectedIndex = _seedOptions.ItemOptions.ContainsKey(itemOptions.Item) ? _seedOptions.ItemOptions[itemOptions.Item] : 0;
            if (selectedIndex < 0 || selectedIndex >= itemOptions.Options.Count)
                selectedIndex = 0;

            var comboBox = new ComboBox
                {
                    ItemsSource = itemOptions.Options,
                    SelectedIndex = selectedIndex,
                    Tag = itemOptions,
                    DisplayMemberPath = "Display"
                };
            comboBox.SelectionChanged += ItemComboBox_Changed;
            _comboBoxes.Add(comboBox);

            var labeledControl = new LabeledControl { Text = itemOptions.Item, Content = comboBox };
            if (itemOptions.IsMetroid)
            {
                MetroidItemsStackPanel.Children.Add(labeledControl);
            }
            else
            {
                ZeldaItemsStackPanel.Children.Add(labeledControl);
            }
        }

        private void ItemComboBox_Changed(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;
            if (comboBox?.Tag is not ItemSettingOptions itemOptions) return;
            var index = comboBox.SelectedIndex;

            if (index == 0)
            {
                _seedOptions.ItemOptions.Remove(itemOptions.Item);
            }
            else
            {
                _seedOptions.ItemOptions[itemOptions.Item] = index;
            }
        }

        private void ItemSettingsPanel_OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (MetroidItemsStackPanel == null) return;
            MetroidItemsStackPanel.Children.Clear();
            ZeldaItemsStackPanel.Children.Clear();
            InitDropdowns();
        }

        private void ResetButton_OnClick(object sender, RoutedEventArgs e)
        {
            _seedOptions.ItemOptions.Clear();
            foreach (var comboBox in _comboBoxes)
            {
                comboBox.SelectedIndex = 0;
            }
        }
    }
}
