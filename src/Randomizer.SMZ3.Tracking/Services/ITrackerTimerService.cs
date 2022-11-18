using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Randomizer.SMZ3.Tracking.Services
{
    /// <summary>
    /// Service for managing the timer and getting the
    /// current time
    /// </summary>
    public interface ITrackerTimerService
    {
        /// <summary>
        /// The total seconds elapsed
        /// </summary>
        double SecondsElapsed { get; }

        /// <summary>
        /// Returns the elapsed time stamp
        /// </summary>
        string TimeString { get; }

        /// <summary>
        /// Timespan for the total elapsed time
        /// </summary>
        TimeSpan TotalElapsedTime { get; }

        /// <summary>
        /// If the timer is currently paused or not
        /// </summary>
        bool IsTimerPaused { get; }

        /// <summary>
        /// Starts/resumes the timer
        /// </summary>
        void StartTimer();

        /// <summary>
        /// Stops/pauses the timer
        /// </summary>
        void StopTimer();

        /// <summary>
        /// Clears the timer and starts it
        /// </summary>
        void ResetTimer();

        /// <summary>
        /// Undoes the last action
        /// </summary>
        void Undo();

        /// <summary>
        /// Sets the saved time used by the timer
        /// to calculate the elapsed time
        /// </summary>
        /// <param name="time">The time to set the timer to</param>
        void SetSavedTime(TimeSpan time);
    }
}
