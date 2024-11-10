using System.Collections.Generic;
using MSURandomizerLibrary;
using TrackerCouncil.Smz3.Shared.Enums;

namespace TrackerCouncil.Smz3.Data.Options;

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
    /// Amount of time in minutes to be able to undo an action
    /// </summary>
    public int UndoExpirationTime { get; set; } = 3;

    /// <summary>
    /// The selected profiles for tracker responses
    /// </summary>
    public ICollection<string?> TrackerProfiles { get; set; } = new List<string?>() { "Sassy" };

    /// <summary>
    /// The output style for the current song (Deprecated)
    /// </summary>
    public MsuTrackDisplayStyle? MsuTrackDisplayStyle { get; set; }

    /// <summary>
    /// The output style for the current song
    /// </summary>
    public TrackDisplayFormat TrackDisplayFormat { get; set; }

    /// <summary>
    /// If the gRPC server for receiving MSU randomizer messages should be enabled
    /// </summary>
    public bool MsuMessageReceiverEnabled { get; set; } = true;

    /// <summary>
    /// The file to write the current song to
    /// </summary>
    public string? MsuTrackOutputPath { get; set; }

    /// <summary>
    /// Automatically tracks the map and other "hey tracker, look at this" events when viewing
    /// </summary>
    public bool AutoSaveLookAtEvents { get; set; }

    /// <summary>
    /// How winners should be determined for the GT guessing game
    /// </summary>
    public GanonsTowerGuessingGameStyle GanonsTowerGuessingGameStyle { get; set; }

    /// <summary>
    /// Speech recognition mode
    /// </summary>
    public SpeechRecognitionMode SpeechRecognitionMode { get; set; }

    /// <summary>
    /// Key to be used for push-to-talk mode
    /// </summary>
    public PushToTalkKey PushToTalkKey { get; set; } = PushToTalkKey.KeyLeftControl;

    /// <summary>
    /// Device to be used for push to talk mode
    /// </summary>
    public string PushToTalkDevice { get; set; } = "Default";

    /// <summary>
    /// Whether the timer should be displayed and timer voice lines should be enabled
    /// </summary>
    public bool TrackerTimerEnabled { get; set; } = true;
}
