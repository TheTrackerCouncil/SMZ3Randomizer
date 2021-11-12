using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Randomizer.App
{
    public class EnumToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) 
            => value.Equals(parameter);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) 
            => (bool)value ? parameter : Binding.DoNothing;
    }
}
