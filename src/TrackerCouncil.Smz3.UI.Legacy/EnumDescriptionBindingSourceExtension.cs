using System;
using System.Linq;
using System.Windows.Markup;
using TrackerCouncil.Smz3.Shared;

namespace TrackerCouncil.Smz3.UI.Legacy;

public class EnumDescriptionBindingSourceExtension : MarkupExtension
{
    private Type? _enumType;

    public EnumDescriptionBindingSourceExtension()
    {
    }

    public EnumDescriptionBindingSourceExtension(Type enumType)
    {
        EnumType = enumType;
    }

    public Type? EnumType
    {
        get => _enumType;
        set
        {
            if (value != _enumType)
            {
                if (null != value)
                {
                    var enumType = Nullable.GetUnderlyingType(value) ?? value;
                    if (!enumType.IsEnum)
                        throw new ArgumentException("Type must be for an Enum.");
                }

                _enumType = value;
            }
        }
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        if (null == _enumType)
            throw new InvalidOperationException("The EnumType must be specified.");

        var actualEnumType = Nullable.GetUnderlyingType(_enumType) ?? _enumType;
        var enumValues = Enum.GetValues(actualEnumType);
        var descriptions = enumValues.Cast<Enum>().Select(x => x.GetDescription());

        if (actualEnumType == _enumType)
            return descriptions;

        var tempArray = new string[enumValues.Length + 1];
        enumValues.CopyTo(tempArray, 1);
        return tempArray;
    }
}
