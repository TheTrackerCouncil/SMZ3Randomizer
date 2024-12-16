using System;
using System.Collections.Generic;
using System.Linq;
using MSURandomizerLibrary.Configs;
using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Data.Tracking;
using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.Data.WorldData.Regions;
using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.Shared.Models;

namespace TrackerCouncil.Smz3.Tracking.TrackingServices;

internal class TrackerGameStateService : TrackerService, ITrackerGameStateService
{
    public bool HasBeatenGame { get; protected set; }
    public ViewedObject? LastViewedObject { get; set; }
    public Region? CurrentRegion { get; protected set; }
    public string CurrentMap { get; protected set; } = "";
    public int CurrentTrackNumber { get; protected set; }

    public event EventHandler? MapUpdated;
    public event EventHandler<TrackerEventArgs>? BeatGame;
    public event EventHandler<TrackerEventArgs>? PlayerDied;
    public event EventHandler<HintTileUpdatedEventArgs>? HintTileUpdated;
    public event EventHandler<TrackNumberEventArgs>? TrackNumberUpdated;
    public event EventHandler<TrackChangedEventArgs>? TrackChanged;

    /// <summary>
    /// Updates the region that the player is in
    /// </summary>
    /// <param name="region">The region the player is in</param>
    /// <param name="updateMap">Set to true to update the map for the player to match the region</param>
    /// <param name="resetTime">If the time should be reset if this is the first region update</param>
    public void UpdateRegion(Region region, AutoMapUpdateBehavior updateMap = AutoMapUpdateBehavior.Disabled, bool resetTime = false)
    {
        if (region != CurrentRegion)
        {
            if (resetTime && !History.GetHistory().Any(x => x is { LocationId: not null, IsUndone: false }))
            {
                Tracker.ResetTimer(true);
            }

            History.AddEvent(
                HistoryEventType.EnteredRegion,
                true,
                region.Name
            );

            if (updateMap != AutoMapUpdateBehavior.Disabled && !string.IsNullOrEmpty(region?.MapName))
            {
                if (updateMap == AutoMapUpdateBehavior.UpdateOnGameChange)
                {
                    if (region is SMRegion && CurrentRegion is not SMRegion)
                    {
                        UpdateMap("Metroid Combined");
                    }
                    else if (region is Z3Region && CurrentRegion is not Z3Region)
                    {
                        UpdateMap("Zelda Combined");
                    }
                }
                else if (updateMap == AutoMapUpdateBehavior.UpdateOnRegionChange)
                {
                    UpdateMap(region.MapName);
                }
                else if (updateMap == AutoMapUpdateBehavior.UpdateOnMetroidRegionChange)
                {
                    if (region is Z3Region && CurrentRegion is not Z3Region)
                    {
                        UpdateMap("Zelda Combined");
                    }
                    else if (region is SMRegion)
                    {
                        UpdateMap(region.MapName);
                    }
                }
                else if (updateMap == AutoMapUpdateBehavior.UpdateOnZeldaRegionChange)
                {
                    if (region is SMRegion && CurrentRegion is not SMRegion)
                    {
                        UpdateMap("Metroid Combined");
                    }
                    else if (region is Z3Region)
                    {
                        UpdateMap(region.MapName);
                    }
                }
            }
        }

        CurrentRegion = region;
    }

    /// <summary>
    /// Updates the map to display for the user
    /// </summary>
    /// <param name="map">The name of the map</param>
    public void UpdateMap(string map)
    {
        CurrentMap = map;
        MapUpdated?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Called when the game is beaten by entering triforce room
    /// or entering the ship after beating both bosses
    /// </summary>
    /// <param name="autoTracked">If this was triggered by the auto tracker</param>
    public void GameBeaten(bool autoTracked)
    {
        if (!HasBeatenGame)
        {
            HasBeatenGame = true;
            var pauseUndo = Tracker.PauseTimer(false);
            Tracker.Say(x => x.BeatGame);
            BeatGame?.Invoke(this, new TrackerEventArgs(autoTracked));
            AddUndo(autoTracked, () =>
            {
                HasBeatenGame = false;
                if (pauseUndo != null)
                {
                    pauseUndo();
                }

                BeatGame?.Invoke(this, new TrackerEventArgs(autoTracked));
            });
        }
    }

    /// <summary>
    /// Called when the player has died
    /// </summary>
    public void TrackDeath(bool autoTracked)
    {
        PlayerDied?.Invoke(this, new TrackerEventArgs(autoTracked));
    }

    /// <summary>
    /// Updates the current track number being played
    /// </summary>
    /// <param name="number">The number of the track</param>
    public void UpdateTrackNumber(int number)
    {
        if (number <= 0 || number > 200 || number == CurrentTrackNumber) return;
        CurrentTrackNumber = number;
        TrackNumberUpdated?.Invoke(this, new TrackNumberEventArgs(number));
    }

    /// <summary>
    /// Updates the current track being played
    /// </summary>
    /// <param name="msu">The current MSU pack</param>
    /// <param name="track">The current track</param>
    /// <param name="outputText">Formatted output text matching the requested style</param>
    public void UpdateTrack(Msu msu, Track track, string outputText)
    {
        TrackChanged?.Invoke(this, new TrackChangedEventArgs(msu, track, outputText));
    }

    public void UpdateHintTile(PlayerHintTile hintTile)
    {
        if (hintTile.State == null || LastViewedObject?.HintTile == hintTile)
        {
            return;
        }
        else if (hintTile.HintState == HintState.Cleared)
        {
            HintTileUpdated?.Invoke(this, new HintTileUpdatedEventArgs(hintTile));
            return;
        }

        LastViewedObject = new ViewedObject { HintTile = hintTile };

        if (hintTile.Type == HintTileType.Location)
        {
            var locationId = hintTile.Locations!.First();
            var location = World.FindLocation(locationId);
            if (location is { Cleared: false, Autotracked: false, HasMarkedCorrectItem: false })
            {
                Tracker.LocationTracker.MarkLocation(location, location.Item, null, true);
                hintTile.HintState = HintState.Viewed;
            }
            else
            {
                hintTile.HintState = HintState.Cleared;
            }
        }
        else if (hintTile.Type == HintTileType.Requirement)
        {
            var dungeon = World.PrerequisiteRegions.First(x => x.Name == hintTile.LocationKey);
            if (!dungeon.HasMarkedCorrectly)
            {
                Tracker.PrerequisiteTracker.SetDungeonRequirement(dungeon, hintTile.MedallionType, null, true);
                hintTile.HintState = HintState.Viewed;
            }
            else
            {
                hintTile.HintState = HintState.Cleared;
            }
        }
        else
        {
            var locations = hintTile.Locations!.Select(x => World.FindLocation(x)).ToList();
            if (locations.All(x => x.Autotracked || x.Cleared))
            {
                hintTile.HintState = HintState.Cleared;
                Tracker.Say(response: Configs.HintTileConfig.ViewedHintTileAlreadyVisited, args: [hintTile.LocationKey]);
            }
            else
            {
                switch (hintTile.Usefulness)
                {
                    case LocationUsefulness.Mandatory:
                        hintTile.HintState = HintState.Viewed;
                        Tracker.Say(response: Configs.HintTileConfig.ViewedHintTileMandatory, args: [hintTile.LocationKey]);
                        break;
                    case LocationUsefulness.Useless:
                        hintTile.HintState = HintState.Viewed;
                        Tracker.Say(response: Configs.HintTileConfig.ViewedHintTileUseless, args: [hintTile.LocationKey]);
                        break;
                    default:
                        hintTile.HintState = HintState.Viewed;
                        Tracker.Say(response: Configs.HintTileConfig.ViewedHintTile);
                        break;
                }

                foreach (var location in locations)
                {
                    location.MarkedUsefulness = hintTile.Usefulness;
                }
            }
        }

        HintTileUpdated?.Invoke(this, new HintTileUpdatedEventArgs(hintTile));
    }

    public void UpdateLastMarkedLocations(List<Location> locations)
    {
        LastViewedObject = new ViewedObject { ViewedLocations = locations };
    }

    public void ClearLastViewedObject(float confidence)
    {
        if (LastViewedObject?.ViewedLocations?.Count > 0)
        {
            Tracker.LocationTracker.Clear(LastViewedObject.ViewedLocations, confidence);
        }
        else if (LastViewedObject?.HintTile != null)
        {
            var hintTile = LastViewedObject.HintTile;

            if (hintTile.State == null)
            {
                Tracker.Say(response: Configs.HintTileConfig.NoPreviousHintTile);
            }
            else if (hintTile.HintState != HintState.Cleared && hintTile.Locations?.Count() > 0)
            {
                var locations = hintTile.Locations.Select(x => World.FindLocation(x))
                    .Where(x => x is { Cleared: false, Autotracked: false }).ToList();
                if (locations.Count != 0)
                {
                    Tracker.LocationTracker.Clear(locations, confidence);
                    hintTile.HintState = HintState.Cleared;
                    UpdateHintTile(hintTile);
                }
                else
                {
                    Tracker.Say(response: Configs.HintTileConfig.ClearHintTileFailed);
                }
            }
            else
            {
                Tracker.Say(response: Configs.HintTileConfig.ClearHintTileFailed);
            }
        }
        else
        {
            Tracker.Say(x => x.NoMarkedLocations);
        }
    }
}
