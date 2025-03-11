using MSURandomizerLibrary.Configs;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Data.Tracking;
using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.Data.WorldData.Regions;
using TrackerCouncil.Smz3.Shared.Models;

namespace TrackerCouncil.Smz3.Abstractions;

public interface ITrackerGameStateService
{
    /// <summary>
    /// Gets if the local player has beaten the game or not
    /// </summary>
    public bool HasBeatenGame { get; }

    /// <summary>
    /// The last viewed hint tile or set of locations
    /// </summary>
    public ViewedObject? LastViewedObject { get; }

    /// <summary>
    /// The region the player is currently in according to the Auto Tracker
    /// </summary>
    public Region? CurrentRegion { get; }

    /// <summary>
    /// The map to display for the player
    /// </summary>
    public string CurrentMap { get; }

    /// <summary>
    /// The current track number being played
    /// </summary>
    public int CurrentTrackNumber { get; }

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
    /// Occurs when a hint tile is viewed that is for a region, dungeon, or group of locations
    /// </summary>
    public event EventHandler<HintTileUpdatedEventArgs>? HintTileUpdated;

    /// <summary>
    /// Occurs when the current played track number is updated
    /// </summary>
    public event EventHandler<TrackNumberEventArgs>? TrackNumberUpdated;

    /// <summary>
    /// Occurs when the current track has changed
    /// </summary>
    public event EventHandler<TrackChangedEventArgs>? TrackChanged;

    /// <summary>
    /// Updates the region that the player is in
    /// </summary>
    /// <param name="region">The region the player is in</param>
    /// <param name="updateMap">Behavior for updating the map for the player to match the region</param>
    /// <param name="resetTime">If the time should be reset if this is the first region update</param>
    public void UpdateRegion(Region region, AutoMapUpdateBehavior updateMap = AutoMapUpdateBehavior.Disabled, bool resetTime = false);

    /// <summary>
    /// Updates the map to display for the user
    /// </summary>
    /// <param name="map">The name of the map</param>
    public void UpdateMap(string map);

    /// <summary>
    /// Called when the game is beaten by entering triforce room
    /// or entering the ship after beating both bosses
    /// </summary>
    /// <param name="autoTracked">If this was triggered by the auto tracker</param>
    public void GameBeaten(bool autoTracked);

    /// <summary>
    /// Called when the player has died
    /// </summary>
    public void TrackDeath(bool autoTracked);

    /// <summary>
    /// Updates the current track number being played
    /// </summary>
    /// <param name="number">The number of the track</param>
    public void UpdateTrackNumber(int number);

    /// <summary>
    /// Updates the current track being played
    /// </summary>
    /// <param name="msu">The current MSU pack</param>
    /// <param name="track">The current track</param>
    /// <param name="outputText">Formatted output text matching the requested style</param>
    public void UpdateTrack(Msu msu, Track track, string outputText);

    public void UpdateHintTile(PlayerHintTile hintTile);

    public void UpdateLastMarkedLocations(List<Location> locations);

    public void ClearLastViewedObject(float confidence);

    public void UpdateGanonsTowerRequirement(int crystalAmount, bool autoTracked);

    public void UpdateGanonRequirement(int crystalAmount, bool autoTracked);

    public void UpdateTourianRequirement(int bossAmount, bool autoTracked);
}
