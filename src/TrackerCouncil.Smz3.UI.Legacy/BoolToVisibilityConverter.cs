using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TrackerCouncil.Smz3.UI.Legacy;

[ValueConversion(typeof(bool), typeof(Visibility))]
public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (bool) value ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter,CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
