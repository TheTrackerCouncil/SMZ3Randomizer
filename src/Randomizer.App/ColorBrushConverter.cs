using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Randomizer.App
{
    [ValueConversion(typeof(byte[]), typeof(SolidColorBrush))]
    internal class ColorBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var rgb = value as byte[];
            return new SolidColorBrush(Color.FromArgb(rgb[0], rgb[1], rgb[2], rgb[3]));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var color = (value as SolidColorBrush).Color;
            return new byte[] { color.A, color.R, color.G, color.B };
        }
    }
}
