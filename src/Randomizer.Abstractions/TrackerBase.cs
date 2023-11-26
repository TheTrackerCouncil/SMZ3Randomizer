using MSURandomizerLibrary.Configs;
using Randomizer.Data;
using Randomizer.Data.Configuration;
using Randomizer.Data.Configuration.ConfigFiles;
using Randomizer.Data.Configuration.ConfigTypes;
using Randomizer.Data.Tracking;
using Randomizer.Data.WorldData;
using Randomizer.Data.WorldData.Regions;
using Randomizer.Shared;
using Randomizer.Shared.Models;

namespace Randomizer.Abstractions;

public abstract class TrackerBase
{
    /// <summary>
    /// Occurs when any speech was recognized, regardless of configured
    /// thresholds.
    /// </summary>
    public event EventHandler<TrackerEventArgs>? SpeechRecognized;

    /// <summary>
    /// Occurs when one more more items have been tracked.
    /// </summary>
    public event EventHandler<ItemTrackedEventArgs>? ItemTracked;

    /// <summary>
    /// Occurs when a location has been cleared.
    /// </summary>
    public event EventHandler<LocationClearedEventArgs>? LocationCleared;

    /// <summary>
    /// Occurs when Peg World mode has been toggled on.
    /// </summary>
    public event EventHandler<TrackerEventArgs>? ToggledPegWorldModeOn;

    /// <summary>
    /// Occurs when going to Shaktool
    /// </summary>
    public event EventHandler<TrackerEventArgs>? ToggledShaktoolMode;

    /// <summary>
    /// Occurs when a Peg World peg has been pegged.
    /// </summary>
    public event EventHandler<TrackerEventArgs>? PegPegged;

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
    /// Occurs when Go mode has been turned on.
    /// </summary>
    public event EventHandler<TrackerEventArgs>? GoModeToggledOn;

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
    /// Gets a reference to the <see cref="ItemService"/>.
    /// </summary>
    public IItemService ItemService { get; protected init; } = null!;

    /// <summary>
    /// The number of pegs that have been pegged for Peg World mode
    /// </summary>
    public int PegsPegged { get; protected set; }

    /// <summary>
    /// Gets the world for the currently tracked playthrough.
    /// </summary>
    public World World { get; protected init; } = null!;

    /// <summary>
    /// Indicates whether Tracker is in Go Mode.
    /// </summary>
    public bool GoMode { get; protected set; }

    /// <summary>
    /// Indicates whether Tracker is in Peg World mode.
    /// </summary>
    public bool PegWorldMode { get; protected set; }

    /// <summary>
    /// Indicates whether Tracker is in Shaktool mode.
    /// </summary>
    public bool ShaktoolMode { get; protected set; }

    /// <summary>
    /// If the speech recognition engine was fully initialized
    /// </summary>
    public bool MicrophoneInitialized { get; protected set; }

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
    public RegionInfo? CurrentRegion { get; protected set; }

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
    protected IHistoryService History { get; init; } = null!;

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
    /// The last viewed hint tile by the player
    /// </summary>
    public PlayerHintTile? LastViewedHintTile { get; set; }

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

    /// <summary>
    /// Undoes the last operation.
    /// </summary>
    /// <param name="confidence">The speech recognition confidence.</param>
    public abstract void Undo(float confidence);

    /// <summary>
    /// Toggles Go Mode on.
    /// </summary>
    /// <param name="confidence">The speech recognition confidence.</param>
    public abstract void ToggleGoMode(float? confidence = null);

    /// <summary>
    /// Removes one or more items from the available treasure in the
    /// specified dungeon.
    /// </summary>
    /// <param name="dungeon">The dungeon.</param>
    /// <param name="amount">The number of treasures to track.</param>
    /// <param name="confidence">The speech recognition confidence.</param>
    /// <param name="autoTracked">If this was called by the auto tracker</param>
    /// <param name="stateResponse">If tracker should state the treasure ammount</param>
    /// <returns>
    /// <c>true</c> if treasure was tracked; <c>false</c> if there is no
    /// treasure left to track.
    /// </returns>
    /// <remarks>
    /// This method adds to the undo history if the return value is
    /// <c>true</c>.
    /// </remarks>
    /// <exception cref=" ArgumentOutOfRangeException">
    /// <paramref name="amount"/> is less than 1.
    /// </exception>
    public abstract bool TrackDungeonTreasure(IDungeon dungeon, float? confidence = null, int amount = 1, bool autoTracked = false, bool stateResponse = true);

    /// <summary>
    /// Sets the dungeon's reward to the specific pendant or crystal.
    /// </summary>
    /// <param name="dungeon">The dungeon to mark.</param>
    /// <param name="reward">
    /// The type of pendant or crystal, or <c>null</c> to cycle through the
    /// possible rewards.
    /// </param>
    /// <param name="confidence">The speech recognition confidence.</param>
    /// <param name="autoTracked">If this was called by the auto tracker</param>
    public abstract void SetDungeonReward(IDungeon dungeon, RewardType? reward = null, float? confidence = null, bool autoTracked = false);

    /// <summary>
    /// Sets the reward of all unmarked dungeons.
    /// </summary>
    /// <param name="reward">The reward to set.</param>
    /// <param name="confidence">The speech recognition confidence.</param>
    public abstract void SetUnmarkedDungeonReward(RewardType reward, float? confidence = null);

    /// <summary>
    /// Sets the dungeon's medallion requirement to the specified item.
    /// </summary>
    /// <param name="dungeon">The dungeon to mark.</param>
    /// <param name="medallion">The medallion that is required.</param>
    /// <param name="confidence">The speech recognition confidence.</param>
    /// <param name="autoTracked">If the marked dungeon requirement was auto tracked</param>
    public abstract void SetDungeonRequirement(IDungeon dungeon, ItemType? medallion = null, float? confidence = null, bool autoTracked = false);

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
    /// <param name="text">The possible sentences to speak.</param>
    /// <returns>
    /// <c>true</c> if a sentence was spoken, <c>false</c> if <paramref
    /// name="text"/> was <c>null</c>.
    /// </returns>
    public abstract bool Say(SchrodingersString? text);

    /// <summary>
    /// Speak a sentence using text-to-speech.
    /// </summary>
    /// <param name="selectResponse">Selects the response to use.</param>
    /// <returns>
    /// <c>true</c> if a sentence was spoken, <c>false</c> if the selected
    /// response was <c>null</c>.
    /// </returns>
    public abstract bool Say(Func<ResponseConfig, SchrodingersString?> selectResponse);

    /// <summary>
    /// Speak a sentence using text-to-speech.
    /// </summary>
    /// <param name="text">The possible sentences to speak.</param>
    /// <param name="args">The arguments used to format the text.</param>
    /// <returns>
    /// <c>true</c> if a sentence was spoken, <c>false</c> if <paramref
    /// name="text"/> was <c>null</c>.
    /// </returns>
    public abstract bool Say(SchrodingersString? text, params object?[] args);

    /// <summary>
    /// Speak a sentence using text-to-speech.
    /// </summary>
    /// <param name="selectResponse">Selects the response to use.</param>
    /// <param name="args">The arguments used to format the text.</param>
    /// <returns>
    /// <c>true</c> if a sentence was spoken, <c>false</c> if the selected
    /// response was <c>null</c>.
    /// </returns>
    public abstract bool Say(Func<ResponseConfig, SchrodingersString?> selectResponse, params object?[] args);

    /// <summary>
    /// Speak a sentence using text-to-speech only one time.
    /// </summary>
    /// <param name="text">The possible sentences to speak.</param>
    /// <returns>
    /// <c>true</c> if a sentence was spoken, <c>false</c> if <paramref
    /// name="text"/> was <c>null</c>.
    /// </returns>
    public abstract bool SayOnce(SchrodingersString? text);

    /// <summary>
    /// Speak a sentence using text-to-speech only one time.
    /// </summary>
    /// <param name="selectResponse">Selects the response to use.</param>
    /// <returns>
    /// <c>true</c> if a sentence was spoken, <c>false</c> if the selected
    /// response was <c>null</c>.
    /// </returns>
    public abstract bool SayOnce(Func<ResponseConfig, SchrodingersString?> selectResponse);

    /// <summary>
    /// Speak a sentence using text-to-speech only one time.
    /// </summary>
    /// <param name="text">The text response to use.</param>
    /// <param name="args">Arguments to substitute out in the text</param>
    /// <returns>
    /// <c>true</c> if a sentence was spoken, <c>false</c> if the selected
    /// response was <c>null</c>.
    /// </returns>
    public abstract bool SayOnce(SchrodingersString? text, params object?[] args);

    /// <summary>
    /// Speak a sentence using text-to-speech only one time.
    /// </summary>
    /// <param name="selectResponse">Selects the response to use.</param>
    /// <param name="args">The arguments used to format the text.</param>
    /// <returns>
    /// <c>true</c> if a sentence was spoken, <c>false</c> if the selected
    /// response was <c>null</c>.
    /// </returns>
    public abstract bool SayOnce(Func<ResponseConfig, SchrodingersString?> selectResponse, params object?[] args);

    /// <summary>
    /// Speak a sentence using text-to-speech.
    /// </summary>
    /// <param name="text">The phrase to speak.</param>
    /// <param name="wait">
    /// <c>true</c> to wait until the text has been spoken completely or
    /// <c>false</c> to immediately return. The default is <c>false</c>.
    /// </param>
    /// <returns>
    /// <c>true</c> if a sentence was spoken, <c>false</c> if the selected
    /// response was <c>null</c>.
    /// </returns>
    public abstract bool Say(string? text, bool wait = false);

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
    /// Tracks the specifies item.
    /// </summary>
    /// <param name="item">The item data to track.</param>
    /// <param name="trackedAs">
    /// The text that was tracked, when triggered by voice command.
    /// </param>
    /// <param name="confidence">The speech recognition confidence.</param>
    /// <param name="tryClear">
    /// <see langword="true"/> to attempt to clear a location for the
    /// tracked item; <see langword="false"/> if that is done by the caller.
    /// </param>
    /// <param name="autoTracked">If this was tracked by the auto tracker</param>
    /// <param name="location">The location an item was tracked from</param>
    /// <param name="giftedItem">If the item was gifted to the player by tracker or another player</param>
    /// <param name="silent">If tracker should not say anything</param>
    /// <returns>
    /// <see langword="true"/> if the item was actually tracked; <see
    /// langword="false"/> if the item could not be tracked, e.g. when
    /// tracking Bow twice.
    /// </returns>
    public abstract bool TrackItem(Item item, string? trackedAs = null, float? confidence = null, bool tryClear = true, bool autoTracked = false, Location? location = null, bool giftedItem = false, bool silent = false);

    /// <summary>
    /// Tracks multiple items at the same time
    /// </summary>
    /// <param name="items">The items to track</param>
    /// <param name="autoTracked">If the items were tracked via auto tracker</param>
    /// <param name="giftedItem">If the items were gifted to the player</param>
    public abstract void TrackItems(List<Item> items, bool autoTracked, bool giftedItem);

    /// <summary>
    /// Removes an item from the tracker.
    /// </summary>
    /// <param name="item">The item to untrack.</param>
    /// <param name="confidence">The speech recognition confidence.</param>
    public abstract void UntrackItem(Item item, float? confidence = null);

    /// <summary>
    /// Tracks the specifies item and clears it from the specified dungeon.
    /// </summary>
    /// <param name="item">The item data to track.</param>
    /// <param name="trackedAs">
    /// The text that was tracked, when triggered by voice command.
    /// </param>
    /// <param name="dungeon">The dungeon the item was tracked in.</param>
    /// <param name="confidence">The speech recognition confidence.</param>
    public abstract void TrackItem(Item item, IDungeon dungeon, string? trackedAs = null, float? confidence = null);

    /// <summary>
    /// Tracks the specified item and clears it from the specified room.
    /// </summary>
    /// <param name="item">The item data to track.</param>
    /// <param name="trackedAs">
    /// The text that was tracked, when triggered by voice command.
    /// </param>
    /// <param name="area">The area the item was found in.</param>
    /// <param name="confidence">The speech recognition confidence.</param>
    public abstract void TrackItem(Item item, IHasLocations area, string? trackedAs = null, float? confidence = null);

    /// <summary>
    /// Sets the item count for the specified item.
    /// </summary>
    /// <param name="item">The item to track.</param>
    /// <param name="count">
    /// The amount of the item that is in the player's inventory now.
    /// </param>
    /// <param name="confidence">The speech recognition confidence.</param>
    public abstract void TrackItemAmount(Item item, int count, float confidence);

    /// <summary>
    /// Clears every item in the specified area, optionally tracking the
    /// cleared items.
    /// </summary>
    /// <param name="area">The area whose items to clear.</param>
    /// <param name="trackItems">
    /// <c>true</c> to track any items found; <c>false</c> to only clear the
    /// affected locations.
    /// </param>
    /// <param name="includeUnavailable">
    /// <c>true</c> to include every item in <paramref name="area"/>, even
    /// those that are not in logic. <c>false</c> to only include chests
    /// available with current items.
    /// </param>
    /// <param name="confidence">The speech recognition confidence.</param>
    /// <param name="assumeKeys">
    /// Set to true to ignore keys when clearing the location.
    /// </param>
    public abstract void ClearArea(IHasLocations area, bool trackItems, bool includeUnavailable = false, float? confidence = null, bool assumeKeys = false);

    /// <summary>
    /// Marks all locations and treasure within a dungeon as cleared.
    /// </summary>
    /// <param name="dungeon">The dungeon to clear.</param>
    /// <param name="confidence">The speech recognition confidence.</param>
    public abstract void ClearDungeon(IDungeon dungeon, float? confidence = null);

    /// <summary>
    /// Clears an item from the specified location.
    /// </summary>
    /// <param name="location">The location to clear.</param>
    /// <param name="confidence">The speech recognition confidence.</param>
    /// <param name="autoTracked">If this was tracked by the auto tracker</param>
    public abstract void Clear(Location location, float? confidence = null, bool autoTracked = false);

    /// <summary>
    /// Clears an item from the specified locations.
    /// </summary>
    /// <param name="locations">The locations to clear.</param>
    /// <param name="confidence">The speech recognition confidence.</param>
    public abstract void Clear(List<Location> locations, float? confidence = null);

    /// <summary>
    /// Marks a dungeon as cleared and, if possible, tracks the boss reward.
    /// </summary>
    /// <param name="dungeon">The dungeon that was cleared.</param>
    /// <param name="confidence">The speech recognition confidence.</param>
    /// <param name="autoTracked">If this was cleared by the auto tracker</param>
    public abstract void MarkDungeonAsCleared(IDungeon dungeon, float? confidence = null, bool autoTracked = false);

    /// <summary>
    /// Marks a boss as defeated.
    /// </summary>
    /// <param name="boss">The boss that was defeated.</param>
    /// <param name="admittedGuilt">
    /// <see langword="true"/> if the command implies the boss was killed;
    /// <see langword="false"/> if the boss was simply "tracked".
    /// </param>
    /// <param name="confidence">The speech recognition confidence.</param>
    /// <param name="autoTracked">If this was tracked by the auto tracker</param>
    public abstract void MarkBossAsDefeated(Boss boss, bool admittedGuilt = true, float? confidence = null, bool autoTracked = false);

    /// <summary>
    /// Un-marks a boss as defeated.
    /// </summary>
    /// <param name="boss">The boss that should be 'revived'.</param>
    /// <param name="confidence">The speech recognition confidence.</param>
    public abstract void MarkBossAsNotDefeated(Boss boss, float? confidence = null);

    /// <summary>
    /// Un-marks a dungeon as cleared and, if possible, untracks the boss
    /// reward.
    /// </summary>
    /// <param name="dungeon">The dungeon that should be un-cleared.</param>
    /// <param name="confidence">The speech recognition confidence.</param>
    public abstract void MarkDungeonAsIncomplete(IDungeon dungeon, float? confidence = null);

    /// <summary>
    /// Marks an item at the specified location.
    /// </summary>
    /// <param name="location">The location to mark.</param>
    /// <param name="item">
    /// The item that is found at <paramref name="location"/>.
    /// </param>
    /// <param name="confidence">The speech recognition confidence.</param>
    /// <param name="autoTracked">If the marked location was auto tracked</param>
    public abstract void MarkLocation(Location location, Item item, float? confidence = null, bool autoTracked = false);

    /// <summary>
    /// Pegs a Peg World peg.
    /// </summary>
    /// <param name="confidence">The speech recognition confidence.</param>
    public abstract void Peg(float? confidence = null);

    /// <summary>
    /// Starts Peg World mode.
    /// </summary>
    /// <param name="confidence">The speech recognition confidence.</param>
    public abstract void StartPegWorldMode(float? confidence = null);

    /// <summary>
    /// Turns Peg World mode off.
    /// </summary>
    /// <param name="confidence">The speech recognition confidence.</param>
    public abstract void StopPegWorldMode(float? confidence = null);

    /// <summary>
    /// Starts Peg World mode.
    /// </summary>
    /// <param name="confidence">The speech recognition confidence.</param>
    public abstract void StartShaktoolMode(float? confidence = null);

    /// <summary>
    /// Turns Peg World mode off.
    /// </summary>
    /// <param name="confidence">The speech recognition confidence.</param>
    public abstract void StopShaktoolMode(float? confidence = null);

    /// <summary>
    /// Updates the region that the player is in
    /// </summary>
    /// <param name="region">The region the player is in</param>
    /// <param name="updateMap">Set to true to update the map for the player to match the region</param>
    /// <param name="resetTime">If the time should be reset if this is the first region update</param>
    public abstract void UpdateRegion(Region region, bool updateMap = false, bool resetTime = false);

    /// <summary>
    /// Updates the region that the player is in
    /// </summary>
    /// <param name="region">The region the player is in</param>
    /// <param name="updateMap">Set to true to update the map for the player to match the region</param>
    /// <param name="resetTime">If the time should be reset if this is the first region update</param>
    public abstract void UpdateRegion(RegionInfo? region, bool updateMap = false, bool resetTime = false);

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
    public abstract void ClearLastMarkedLocations(float confidence);

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

    /// <summary>
    /// Determines whether or not the specified reward is worth getting.
    /// </summary>
    /// <param name="reward">The dungeon reward.</param>
    /// <returns>
    /// <see langword="true"/> if the reward leads to something good;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public abstract bool IsWorth(RewardType reward);

    /// <summary>
    /// Formats a string so that it will be pronounced correctly by the
    /// text-to-speech engine.
    /// </summary>
    /// <param name="name">The text to correct.</param>
    /// <returns>A string with the pronunciations replaced.</returns>
    public static string CorrectPronunciation(string name)
        => name.Replace("Samus", "Sammus");

    /// <summary>
    /// Determines whether or not the specified item is worth getting.
    /// </summary>
    /// <param name="item">The item whose worth to consider.</param>
    /// <returns>
    /// <see langword="true"/> is the item is worth getting or leads to
    /// another item that is worth getting; otherwise, <see
    /// langword="false"/>.
    /// </returns>
    public abstract bool IsWorth(Item item);

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
    /// Invokes the ToggledPegWorldModeOn event
    /// </summary>
    /// <param name="args"></param>
    protected virtual void OnToggledPegWorldModeOn(TrackerEventArgs args)
    {
        ToggledPegWorldModeOn?.Invoke(this, args);
    }

    /// <summary>
    /// Invokes the ToggledShaktoolMode event
    /// </summary>
    /// <param name="args"></param>
    protected virtual void OnToggledShaktoolMode(TrackerEventArgs args)
    {
        ToggledShaktoolMode?.Invoke(this, args);
    }

    /// <summary>
    /// Invokes the PegPegged event
    /// </summary>
    /// <param name="args"></param>
    protected virtual void OnPegPegged(TrackerEventArgs args)
    {
        PegPegged?.Invoke(this, args);
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
    /// Invokes the GoModeToggledOn event
    /// </summary>
    /// <param name="args"></param>
    protected virtual void OnGoModeToggledOn(TrackerEventArgs args)
    {
        GoModeToggledOn?.Invoke(this, args);
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
}
