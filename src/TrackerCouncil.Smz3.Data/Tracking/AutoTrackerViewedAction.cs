using System;
using System.Threading.Tasks;

namespace TrackerCouncil.Smz3.Data.Tracking;

/// <summary>
/// Class for storing an action from viewing something
/// for a short amount of time
/// </summary>
public class AutoTrackerViewedAction
{
    private Action? _action;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="action"></param>
    public AutoTrackerViewedAction(Action action)
    {
        _action = action;
        _ = ExpireAsync();
    }

    /// <summary>
    /// Expires the action after a period of time
    /// </summary>
    private async Task ExpireAsync()
    {
        await Task.Delay(TimeSpan.FromSeconds(15));
        _action = null;
    }

    /// <summary>
    /// Invokes the action, if it's still valid
    /// </summary>
    /// <returns></returns>
    public bool Invoke()
    {
        if (_action == null) return false;
        _action.Invoke();
        return true;
    }

    /// <summary>
    /// If the action is valid
    /// </summary>
    public bool IsValid => _action != null;
}
