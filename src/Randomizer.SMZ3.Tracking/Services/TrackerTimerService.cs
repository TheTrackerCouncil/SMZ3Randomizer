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
    public class TrackerTimerService : ITrackerTimerService
    {
        private DateTime _startTime = DateTime.MinValue;
        private TimeSpan _savedElapsedTime = TimeSpan.Zero;
        private DateTime _undoStartTime = DateTime.MinValue;
        private TimeSpan _undoSavedTime = TimeSpan.Zero;

        /// <summary>
        /// The total seconds elapsed
        /// </summary>
        public double SecondsElapsed => TotalElapsedTime.TotalSeconds;

        /// <summary>
        /// Returns the elapsed time stamp
        /// </summary>
        public string TimeString => TotalElapsedTime.Hours > 0
                                    ? TotalElapsedTime.ToString("h':'mm':'ss")
                                    : TotalElapsedTime.ToString("mm':'ss");

        /// <summary>
        /// Timespan for the total elapsed time
        /// </summary>
        public TimeSpan TotalElapsedTime => IsTimerPaused ? _savedElapsedTime : _savedElapsedTime + (DateTime.Now - _startTime);

        /// <summary>
        /// If the timer is currently paused or not
        /// </summary>
        public bool IsTimerPaused => _startTime == DateTime.MinValue;

        /// <summary>
        /// Clears the timer and starts it
        /// </summary>
        public void ResetTimer()
        {
            SaveUndoTime();
            _savedElapsedTime = TimeSpan.Zero;
            _startTime = DateTime.Now;
        }

        /// <summary>
        /// Sets the saved time used by the timer
        /// to calculate the elapsed time
        /// </summary>
        /// <param name="time">The time to set the timer to</param>
        public void SetSavedTime(TimeSpan time)
        {
            _savedElapsedTime = time;
        }

        /// <summary>
        /// Starts/resumes the timer
        /// </summary>
        public void StartTimer()
        {
            SaveUndoTime();
            _startTime = DateTime.Now;
        }

        /// <summary>
        /// Stops/pauses the timer
        /// </summary>
        public void StopTimer()
        {
            SaveUndoTime();
            _savedElapsedTime = TotalElapsedTime;
            _startTime = DateTime.MinValue;
        }

        /// <summary>
        /// Undoes the last action
        /// </summary>
        public void Undo()
        {
            if (_undoStartTime != DateTime.MinValue || _undoSavedTime != TimeSpan.Zero)
            {
                _savedElapsedTime = _undoSavedTime;
                _startTime = _undoStartTime;
                _undoStartTime = DateTime.MinValue;
                _undoSavedTime = TimeSpan.Zero;
            }
        }

        private void SaveUndoTime()
        {
            _undoSavedTime = _savedElapsedTime;
            _undoStartTime = _startTime;
        }
    }
}
