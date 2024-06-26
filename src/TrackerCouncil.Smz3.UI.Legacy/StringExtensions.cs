namespace TrackerCouncil.Smz3.UI.Legacy;

public static class StringExtensions
{
    public static string Or(this string value, string fallbackValue)
        => string.IsNullOrEmpty(value) ? fallbackValue : value;
}
