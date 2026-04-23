using System;

namespace TrackerCouncil.Smz3.Shared;

public static class EnumUtils
{
    public static T Random<T>(Random rnd) where T : struct, Enum
    {
        var values = Enum.GetValues<T>();
        return values[rnd.Next(values.Length)];
    }

    public static T RandomExcludingLast<T>(Random rnd) where T : struct, Enum
    {
        var values = Enum.GetValues<T>();
        return values.Length <= 1 ? values[0] : values[rnd.Next(values.Length - 1)];
    }
}
