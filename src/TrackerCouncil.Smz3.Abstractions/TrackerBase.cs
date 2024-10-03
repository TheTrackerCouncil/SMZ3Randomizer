using MSURandomizerLibrary.Configs;
using TrackerCouncil.Smz3.Data.Configuration;
using TrackerCouncil.Smz3.Data.Configuration.ConfigFiles;
using TrackerCouncil.Smz3.Data.Configuration.ConfigTypes;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Data.Tracking;
using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.Data.WorldData.Regions;
using TrackerCouncil.Smz3.Shared.Models;

namespace TrackerCouncil.Smz3.Abstractions;

public abstract class TrackerBase
{
    /// <summary>
    /// Occurs when any speech was recognized, regardless of configured
    /// thresholds.
    /// </summary>
    public event EventHandler<TrackerEventArgs>? SpeechRecognized;

    /// <summary>
    /// Occurs when one or more items have been tracked.
    /// </summary>
    public event EventHandler<ItemTrackedEventArgs>? ItemTracked;

    /// <summary>
    /// Occurs when a location has been cleared.
    /// </summary>
    public event EventHandler<LocationClearedEventArgs>? LocationCleared;

    /// <summary>
    /// Occurs when the properties of a dungeon have changed.
    /// </summary>
    public event EventHandler<DungeonTrackedEventArgs>? DungeonUpdated;

    /// <summary>
    /// Occurs when the properties of a boss have changed.
    /// </summary>
    public event EventHandler<BossTrackedEventArgs>? BossUpdated;

    /// <summary>
    /// Occurs when the marked locations have changed
    /// </summary>
    public event EventHandler<TrackerEventArgs>? MarkedLocationsUpdated;

    /// <summary>
    /// Occurs when the last action was undone.
    /// </summary>
    public event EventHandler<TrackerEventArgs>? ActionUndone;

    /// <summary>
    /// Occurs when the tracker state has been loaded.
    /// </summary>
    public event EventHandler? StateLoaded;

    /// <summary>
    /// Occurs when the map has been updated
    /// </summary>
    public event EventHandler? MapUpdated;

    /// <summary>
    /// Occurs when the map has been updated
    /// </summary>
    public event EventHandler<TrackerEventArgs>? BeatGame;

    /// <summary>
    /// Occurs when the map has died
    /// </summary>
    public event EventHandler<TrackerEventArgs>? PlayerDied;

    /// <summary>
    /// Occurs when the current played track number is updated
    /// </summary>
    public event EventHandler<TrackNumberEventArgs>? TrackNumberUpdated;

    /// <summary>
    /// Occurs when the current track has changed
    /// </summary>
    public event EventHandler<TrackChangedEventArgs>? TrackChanged;

    /// <summary>
    /// Occurs when a hint tile is viewed that is for a region, dungeon, or group of locations
    /// </summary>
    public event EventHandler<HintTileUpdatedEventArgs>? HintTileUpdated;

    /// <summary>
    /// Occurs when the voice recognition has been enabled or disabled
    /// </summary>
    public event EventHandler? VoiceRecognitionEnabledChanged;

    /// <summary>
    /// Gets a reference to the <see cref="ItemService"/>.
    /// </summary>
    public IItemService ItemService { get; protected init; } = null!;

    public ITrackerTreasureService TreasureTracker { get; protected set; } = null!;

    public ITrackerItemService ItemTracker { get; protected set; } = null!;

    public ITrackerLocationService LocationTracker { get; protected set; } = null!;

    public ITrackerRewardService RewardTracker { get; protected set; } = null!;

    public ITrackerBossService BossTracker { get; protected set; } = null!;

    public ITrackerPrerequisiteService PrerequisiteTracker { get; protected set; } = null!;

    public ITrackerModeService ModeTracker { get; protected set; } = null!;

    /// <summary>
    /// Gets the world for the currently tracked playthrough.
    /// </summary>
    public World World { get; protected init; } = null!;

    /// <summary>
    /// If the speech recognition engine was fully initialized
    /// </summary>
    public bool MicrophoneInitialized { get; protected set; }

    /// <summary>
    /// If the speech recognition engine was fully initialized with the requested device
    /// </summary>
    public bool MicrophoneInitializedAsDesiredDevice { get; protected set; }

    /// <summary>
    /// If voice recognition has been enabled or not
    /// </summary>
    public bool VoiceRecognitionEnabled { get; protected set; }

    /// <summary>
    /// Gets the configured responses.
    /// </summary>
    public ResponseConfig Responses { get; protected init; } = null!;

    /// <summary>
    /// Gets a collection of basic requests and responses.
    /// </summary>
    public IReadOnlyCollection<BasicVoiceRequest> Requests { get; protected init; } = null!;

    /// <summary>
    /// Metadata configs
    /// </summary>
    public Configs Configs { get; protected init; } = null!;

    /// <summary>
    /// Gets a dictionary containing the rules and the various speech
    /// recognition syntaxes.
    /// </summary>
    public IReadOnlyDictionary<string, IEnumerable<string>> Syntax { get; protected set; } = new Dictionary<string, IEnumerable<string>>();

    /// <summary>
    /// Gets the tracking preferences.
    /// </summary>
    public TrackerOptions Options { get; protected init; } = null!;

    /// <summary>
    /// The generated rom
    /// </summary>
    public GeneratedRom? Rom { get; protected set; }

    /// <summary>
    /// The path to the generated rom
    /// </summary>
    public string? RomPath { get; protected set; }

    /// <summary>
    /// The region the player is currently in according to the Auto Tracker
    /// </summary>
    public Region? CurrentRegion { get; protected set; }

    /// <summary>
    /// The map to display for the player
    /// </summary>
    public string CurrentMap { get; protected set; } = "";

    /// <summary>
    /// The current track number being played
    /// </summary>
    public int CurrentTrackNumber { get; protected set; }

    /// <summary>
    /// Gets a string describing tracker's mood.
    /// </summary>
    public string Mood { get; protected set; } = "";

    /// <summary>
    /// Gets the config used to generate the local seed.
    /// </summary>
    public Config? LocalConfig { get; protected set; }

    /// <summary>
    /// Get if the Tracker has been updated since it was last saved
    /// </summary>
    public bool IsDirty { get; protected set; }

    /// <summary>
    /// The Auto Tracker for the Tracker
    /// </summary>
    public AutoTrackerBase? AutoTracker { get; set; }

    /// <summary>
    /// Service that handles modifying the game via auto tracker
    /// </summary>
    public IGameService? GameService { get; set; }

    /// <summary>
    /// Module that houses the history
    /// </summary>
    public IHistoryService History { get; init; } = null!;

    /// <summary>
    /// Gets or sets a value indicating whether Tracker may give hints when
    /// asked about items or locations.
    /// </summary>
    public bool HintsEnabled { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether Tracker may give spoilers
    /// when asked about items or locations.
    /// </summary>
    public bool SpoilersEnabled { get; set; }

    /// <summary>
    /// Gets if the local player has beaten the game or not
    /// </summary>
    public bool HasBeatenGame { get; protected set; }

    /// <summary>
    /// The last viewed hint tile or set of locations
    /// </summary>
    public ViewedObject? LastViewedObject { get; set; }

    /// <summary>
    /// Attempts to replace a user name with a pronunciation-corrected
    /// version of it.
    /// </summary>
    /// <param name="userName">The user name to correct.</param>
    /// <returns>
    /// The corrected user name, or <paramref name="userName"/>.
    /// </returns>
    public abstract string CorrectUserNamePronunciation(string userName);

    /// <summary>
    /// Initializes the microphone from the default audio device
    /// </summary>
    /// <returns>
    /// True if the microphone is initialized, false otherwise
    /// </returns>
    public abstract bool InitializeMicrophone();

    /// <summary>
    /// Loads the state from the database for a given rom
    /// </summary>
    /// <param name="rom">The rom to load</param>
    /// <param name="romPath">The full path to the rom to load</param>
    /// <returns>True or false if the load was successful</returns>
    public abstract bool Load(GeneratedRom rom, string romPath);

    /// <summary>
    /// Saves the state of the tracker to the database
    /// </summary>
    /// <returns></returns>
    public abstract Task SaveAsync();

    public abstract void MarkAsDirty(bool isDirty = true);

    /// <summary>
    /// Undoes the last operation.
    /// </summary>
    /// <param name="confidence">The speech recognition confidence.</param>
    public abstract void Undo(float confidence);

    /// <summary>
    /// Starts voice recognition.
    /// </summary>
    public abstract bool TryStartTracking();

    /// <summary>
    /// Connects Tracker to chat.
    /// </summary>
    /// <param name="userName">The user name to connect as.</param>
    /// <param name="oauthToken">
    /// The OAuth token for <paramref name="userName"/>.
    /// </param>
    /// <param name="channel">
    /// The channel to monitor for incoming messages.
    /// </param>
    /// <param name="id">
    /// The is for <paramref name="userName"/>.
    /// </param>
    public abstract void ConnectToChat(string? userName, string? oauthToken, string? channel, string? id);

    /// <summary>
    /// Sets the start time of the timer
    /// </summary>
    public abstract void StartTimer(bool isInitial = false);

    /// <summary>
    /// Resets the timer to 0
    /// </summary>
    public abstract void ResetTimer(bool isInitial = false);

    /// <summary>
    /// Pauses the timer, saving the elapsed time
    /// </summary>
    public abstract Action? PauseTimer(bool addUndo = true);

    /// <summary>
    /// Pauses or resumes the timer based on if it is
    /// currently paused or not
    /// </summary>
    public abstract void ToggleTimer();

    /// <summary>
    /// Stops voice recognition.
    /// </summary>
    public abstract void StopTracking();

    /// <summary>
    /// Enables the voice recognizer if the microphone is enabled
    /// </summary>
    public abstract void EnableVoiceRecognition();

    /// <summary>
    /// Disables voice recognition if it was previously enabled
    /// </summary>
    public abstract void DisableVoiceRecognition();

    /// <summary>
    /// Speak a sentence using text-to-speech.
    /// </summary>
    /// <param name="selectResponse">Selects the response to use.</param>
    /// <param name="selectResponses">
    /// Selects the dictionary of responses that <c>key</c> or <c>tieredKey</c> will pull a response from.
    /// </param>
    /// <param name="response">The response to use.</param>
    /// <param name="responses">
    /// The dictionary of responses that <c>key</c> or <c>tieredKey</c> will pull a response from.
    /// </param>
    /// <param name="key">
    /// The exact key of the response that should be pulled from the dictionary of <c>responses</c>.
    /// </param>
    /// <param name="tieredKey">
    /// The "tiered" key of the response that should be pulled from the dictionary of <c>responses</c>.
    /// The closest key in the dictionary that is less than or equal to this value will be used.
    /// </param>
    /// <param name="args">
    /// The arguments used to format the selected response.
    /// This paremeter is ignored when the <c>text</c> parameter is specified.
    /// </param>
    /// <param name="text">The specific sentence that should be spoken, which may include placeholders.</param>
    /// <param name="once">
    /// <c>true</c> to speak the selected response only once or
    /// <c>false</c> to speak it as many times as requested. The default is <c>false</c>.
    /// This parameter is ignored when the <c>text</c> parameter is specified.
    /// </param>
    /// <param name="wait">
    /// <c>true</c> to wait until the text has been spoken completely or
    /// <c>false</c> to immediately return. The default is <c>false</c>.
    /// </param>
    /// <returns>
    /// <c>true</c> if a sentence was spoken, <c>false</c> if the selected
    /// response was <c>null</c>.
    /// </returns>
    public abstract bool Say(
        Func<ResponseConfig, SchrodingersString?>? selectResponse = null,
        Func<ResponseConfig, Dictionary<int, SchrodingersString>?>? selectResponses = null,
        SchrodingersString? response = null,
        Dictionary<int, SchrodingersString>? responses = null,
        int? key = null,
        int? tieredKey = null,
        object?[]? args = null,
        string? text = null,
        bool once = false,
        bool wait = false);

    /// <summary>
    /// Repeats the most recently spoken sentence using text-to-speech at a
    /// slower rate.
    /// </summary>
    public abstract void Repeat();

    /// <summary>
    /// Makes Tracker stop talking.
    /// </summary>
    public abstract void ShutUp();

    /// <summary>
    /// Notifies the user an error occurred.
    /// </summary>
    public abstract void Error();

    /// <summary>
    /// Updates the region that the player is in
    /// </summary>
    /// <param name="region">The region the player is in</param>
    /// <param name="updateMap">Set to true to update the map for the player to match the region</param>
    /// <param name="resetTime">If the time should be reset if this is the first region update</param>
    public abstract void UpdateRegion(Region region, bool updateMap = false, bool resetTime = false);

    /// <summary>
    /// Updates the map to display for the user
    /// </summary>
    /// <param name="map">The name of the map</param>
    public abstract void UpdateMap(string map);

    /// <summary>
    /// Called when the game is beaten by entering triforce room
    /// or entering the ship after beating both bosses
    /// </summary>
    /// <param name="autoTracked">If this was triggered by the auto tracker</param>
    public abstract void GameBeaten(bool autoTracked);

    /// <summary>
    /// Called when the player has died
    /// </summary>
    public abstract void TrackDeath(bool autoTracked);

    /// <summary>
    /// Updates the current track number being played
    /// </summary>
    /// <param name="number">The number of the track</param>
    public abstract void UpdateTrackNumber(int number);

    /// <summary>
    /// Updates the current track being played
    /// </summary>
    /// <param name="msu">The current MSU pack</param>
    /// <param name="track">The current track</param>
    /// <param name="outputText">Formatted output text matching the requested style</param>
    public abstract void UpdateTrack(Msu msu, Track track, string outputText);

    /// <summary>
    /// Marks a hint tile as viewed or cleared
    /// </summary>
    /// <param name="playerHintTile">Details about the hint for the player</param>
    public abstract void UpdateHintTile(PlayerHintTile playerHintTile);

    /// <summary>
    /// Updates the most recently marked locations to be able to clear later
    /// </summary>
    /// <param name="locations">List of locations that were just marked</param>
    public abstract void UpdateLastMarkedLocations(List<Location> locations);

    /// <summary>
    /// Clears the most recently marked locations
    /// </summary>
    /// <param name="confidence">Voice recognition confidence</param>
    public abstract void ClearLastViewedObject(float confidence);

    /// <summary>
    /// Reports how many Hyper Beam shots were needed to defeat Mother Brain
    /// </summary>
    public abstract void CountHyperBeamShots(int count);

    /// <summary>
    /// Resets the idle timers when tracker will comment on nothing happening
    /// </summary>
    public abstract void RestartIdleTimers();

    /// <summary>
    /// Adds an action to be invoked to undo the last operation.
    /// </summary>
    /// <param name="undo">
    /// The action to invoke to undo the last operation.
    /// </param>
    public abstract void AddUndo(Action undo);

    public abstract (Action Action, DateTime UndoTime) PopUndo();

    /// <summary>
    /// Formats a string so that it will be pronounced correctly by the
    /// text-to-speech engine.
    /// </summary>
    /// <param name="name">The text to correct.</param>
    /// <returns>A string with the pronunciations replaced.</returns>
    public static string CorrectPronunciation(string name)
        => name.Replace("Samus", "Sammus");

    /// <summary>
    /// Invokes the SpeechRecognized event
    /// </summary>
    /// <param name="args"></param>
    protected virtual void OnSpeechRecognized(TrackerEventArgs args)
    {
        SpeechRecognized?.Invoke(this, args);
    }

    /// <summary>
    /// Invokes the ItemTracked event
    /// </summary>
    /// <param name="args"></param>
    protected virtual void OnItemTracked(ItemTrackedEventArgs args)
    {
        ItemTracked?.Invoke(this, args);
    }

    /// <summary>
    /// Invokes the LocationCleared event
    /// </summary>
    /// <param name="args"></param>
    protected virtual void OnLocationCleared(LocationClearedEventArgs args)
    {
        LocationCleared?.Invoke(this, args);
    }

    /// <summary>
    /// Invokes the DungeonUpdated event
    /// </summary>
    /// <param name="args"></param>
    protected virtual void OnDungeonUpdated(DungeonTrackedEventArgs args)
    {
        DungeonUpdated?.Invoke(this, args);
    }

    /// <summary>
    /// Invokes the BossUpdated event
    /// </summary>
    /// <param name="args"></param>
    protected virtual void OnBossUpdated(BossTrackedEventArgs args)
    {
        BossUpdated?.Invoke(this, args);
    }

    /// <summary>
    /// Invokes the MarkedLocationsUpdated event
    /// </summary>
    /// <param name="args"></param>
    protected virtual void OnMarkedLocationsUpdated(TrackerEventArgs args)
    {
        MarkedLocationsUpdated?.Invoke(this, args);
    }

    /// <summary>
    /// Invokes the ActionUndone event
    /// </summary>
    /// <param name="args"></param>
    protected virtual void OnActionUndone(TrackerEventArgs args)
    {
        ActionUndone?.Invoke(this, args);
    }

    /// <summary>
    /// Invokes the StateLoaded event
    /// </summary>
    protected virtual void OnStateLoaded()
    {
        StateLoaded?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Invokes the MapUpdated event
    /// </summary>
    protected virtual void OnMapUpdated()
    {
        MapUpdated?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Invokes the BeatGame event
    /// </summary>
    /// <param name="args"></param>
    protected virtual void OnBeatGame(TrackerEventArgs args)
    {
        BeatGame?.Invoke(this, args);
    }

    /// <summary>
    /// Invokes the PlayerDied event
    /// </summary>
    /// <param name="args"></param>
    protected virtual void OnPlayerDied(TrackerEventArgs args)
    {
        PlayerDied?.Invoke(this, args);
    }

    /// <summary>
    /// Invokes the TrackNumberUpdated event
    /// </summary>
    /// <param name="args"></param>
    protected virtual void OnTrackNumberUpdated(TrackNumberEventArgs args)
    {
        TrackNumberUpdated?.Invoke(this, args);
    }

    /// <summary>
    /// Invokes the TrackChanged event
    /// </summary>
    /// <param name="args"></param>
    protected virtual void OnTrackChanged(TrackChangedEventArgs args)
    {
        TrackChanged?.Invoke(this, args);
    }

    /// <summary>
    /// Invokes the HintTileUpdated event
    /// </summary>
    /// <param name="args"></param>
    protected virtual void OnHintTileUpdated(HintTileUpdatedEventArgs args)
    {
        HintTileUpdated?.Invoke(this, args);
    }

    /// <summary>
    /// Invokes the VoiceRecognitionEnabledChanged event
    /// </summary>
    protected virtual void OnVoiceRecognitionEnabledChanged()
    {
        VoiceRecognitionEnabledChanged?.Invoke(this, EventArgs.Empty);
    }
}
