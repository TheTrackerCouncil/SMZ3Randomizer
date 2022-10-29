using System;
using System.ComponentModel;
using System.Reflection;

namespace Randomizer.Shared
{
    public class EnumDescriptionTypeConverter : EnumConverter
    {
        public EnumDescriptionTypeConverter(Type type)
            : base(type)
        {
        }

        public override object? ConvertTo(ITypeDescriptorContext? context, System.Globalization.CultureInfo? culture, object? value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                if (value == null) return string.Empty;
                var fi = value.GetType().GetField(value?.ToString() ?? string.Empty);
                if (fi == null) return string.Empty;
                var attribute = fi.GetCustomAttribute<DescriptionAttribute>(inherit: false);
                return attribute?.Description ?? value?.ToString() ?? string.Empty;
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
