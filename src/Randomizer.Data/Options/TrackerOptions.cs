using System;
using System.Collections.Generic;
using Randomizer.Shared.Enums;

namespace Randomizer.Data
{
    /// <summary>
    /// Provides Tracker preferences.
    /// </summary>
    public record TrackerOptions
    {
        /// <summary>
        /// Gets or sets the minimum confidence level for Tracker to recognize
        /// voice commands.
        /// </summary>
        /// <remarks>
        /// Recognized speech below this threshold will not be executed.
        /// </remarks>
        public float MinimumRecognitionConfidence { get; set; } = 0.75f;

        /// <summary>
        /// Gets or sets the minimum confidence level for Tracker to execute to
        /// voice commands.
        /// </summary>
        /// <remarks>
        /// Recognized speech below this threshold will not be executed, but may
        /// prompt Tracker to ask to repeat.
        /// </remarks>
        public float MinimumExecutionConfidence { get; set; } = 0.85f;

        /// <summary>
        /// Gets or sets the minimum confidence level for Tracker to use sassy
        /// responses or responses that could potentially give away information.
        /// </summary>
        public float MinimumSassConfidence { get; set; } = 0.92f;

        /// <summary>
        /// Gets or sets a value indicating whether Tracker can give hints when
        /// asked about an item or location.
        /// </summary>
        public bool HintsEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Tracker can give spoilers
        /// when asked about an item or location.
        /// </summary>
        public bool SpoilersEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Tracker will respond to
        /// people saying hi to her in chat.
        /// </summary>
        public bool ChatGreetingEnabled { get; set; }

        /// <summary>
        /// Gets or sets the number of minutes Tracker will respond to greetings
        /// in chat after Tracker starts.
        /// </summary>
        public int ChatGreetingTimeLimit { get; set; }

        /// <summary>
        /// Gets or sets the name of the current user.
        /// </summary>
        public string? UserName { get; set; }

        /// <summary>
        /// Gets or sets the number of times Tracker will tolerate being
        /// interrupted before speaking up.
        /// </summary>
        public int InterruptionTolerance { get; set; } = 2;

        /// <summary>
        /// Gets or sets the number of times Tracker will tolerate being
        /// interrupted before quitting.
        /// </summary>
        public int InterruptionLimit { get; set; } = 5;

        /// <summary>
        /// If tracker can create chat polls
        /// </summary>
        public bool PollCreationEnabled { get; set; }

        /// <summary>
        /// If auto tracker should change maps when changing locations
        /// </summary>
        public bool AutoTrackerChangeMap { get; set; }

        /// <summary>
        /// The frequency in which tracker will say things
        /// </summary>
        public TrackerVoiceFrequency VoiceFrequency { get; set; }

        /// <summary>
        /// The selected profiles for tracker responses
        /// </summary>
        public ICollection<string> TrackerProfiles { get; set; } = new List<string>() { "Sassy" };
    }
}
