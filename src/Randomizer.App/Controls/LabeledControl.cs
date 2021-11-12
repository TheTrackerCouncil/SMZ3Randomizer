using System.Windows;
using System.Windows.Controls;

namespace Randomizer.App.Controls
{
    public class LabeledControl : ContentControl
    {
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text",
                propertyType: typeof(string),
                ownerType: typeof(LabeledControl),
                typeMetadata: new PropertyMetadata("Label"));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
    }
}
