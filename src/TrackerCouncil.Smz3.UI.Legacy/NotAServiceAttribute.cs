using System;

namespace TrackerCouncil.Smz3.UI.Legacy;

/// <summary>
/// Specifies that a window should not be added as a service.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
internal class NotAServiceAttribute : Attribute
{
    public NotAServiceAttribute()
    {
    }
}
