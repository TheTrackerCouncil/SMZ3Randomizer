using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using TrackerCouncil.Smz3.Shared;

namespace TrackerCouncil.Smz3.UI.Legacy;

public class EnumDescriptionConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var enumValue = (Enum)value!;
        return enumValue.GetDescription();
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var enumValues = Enum.GetValues(targetType);
        if (value == null)
        {
            return enumValues.GetValue(0)!;
        }
        var descriptions = enumValues.Cast<Enum>().ToDictionary(x => x.GetDescription() as object, x => x);
        return descriptions[value];
    }
}
