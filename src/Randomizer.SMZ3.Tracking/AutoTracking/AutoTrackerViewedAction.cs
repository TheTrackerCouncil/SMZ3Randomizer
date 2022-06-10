using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Randomizer.SMZ3.Tracking.AutoTracking
{
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
            Expire();
        }

        /// <summary>
        /// Expires the action after a period of time
        /// </summary>
        private async void Expire()
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
}
