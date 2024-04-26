using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia;
using AvaloniaControls.ControlServices;
using Microsoft.Extensions.Logging;
using Randomizer.Abstractions;
using Randomizer.CrossPlatform.ViewModels;
using Randomizer.CrossPlatform.Views;
using Randomizer.Data.Configuration.ConfigFiles;
using Randomizer.Data.Configuration.ConfigTypes;
using Randomizer.Data.Options;
using Randomizer.Data.WorldData.Regions;
using Randomizer.Data.WorldData.Regions.Zelda;
using Randomizer.Shared;
using Randomizer.Shared.Enums;
using Randomizer.SMZ3.Contracts;
using Randomizer.SMZ3.Tracking.Services;

namespace Randomizer.CrossPlatform.Services;

public class TrackerMapWindowService(
    TrackerBase tracker,
    TrackerMapConfig trackerMapConfig,
    IWorldAccessor worldAccessor,
    IWorldService worldService,
    IItemService itemService) : ControlService
{
    private TrackerMapWindowViewModel _model = new();
    private Dictionary<TrackerMap, List<TrackerMapLocationViewModel>> _mapLocations = new();

    public TrackerMapWindowViewModel GetViewModel()
    {
        _model.Maps = trackerMapConfig.Maps.ToList();

        var locations = worldAccessor.World.Locations.ToList();

        // To make querying easier for the viewmodel, compile a list of all
        // locations on each map by combining all of their regions, making
        // any location adjusments needed
        foreach (var map in _model.Maps)
        {
            var locationModels = new List<TrackerMapLocationViewModel>();

            foreach (var mapRegion in map.Regions)
            {
                var configRegion = trackerMapConfig.Regions.First(region => mapRegion.Name == region.Name);
                var worldRegion = worldService.Region(configRegion.TypeName) ?? throw new InvalidOperationException();

                var regionLocations = locations.Where(loc => configRegion.TypeName == loc.Region.GetType().FullName).ToList();

                if (configRegion.Rooms != null)
                {
                    locationModels.AddRange(configRegion.Rooms.Select(room =>
                        new TrackerMapLocationViewModel(configRegion,
                            mapRegion, room, worldRegion,
                            regionLocations.Where(loc => room.Name == loc.Name ||
                                                         room.Name == loc.Room?.Name ||
                                                         configRegion.Name == room.Name).ToList())));
                }

                if (configRegion is { BossX: not null, BossY: not null })
                {
                    locationModels.Add(new TrackerMapLocationViewModel(configRegion, mapRegion, worldRegion));
                }

                if (configRegion.Doors != null)
                {
                    locationModels.AddRange(configRegion.Doors.Select(door =>
                        new TrackerMapLocationViewModel(configRegion, mapRegion, worldRegion, door)).ToList());
                }
            }

            _mapLocations[map] = locationModels;
        }

        tracker.LocationCleared += (_, _) => UpdateLocations();
        tracker.DungeonUpdated += (_, _) => UpdateLocations();
        tracker.ItemTracked += (_, _) => UpdateLocations();
        tracker.ActionUndone += (_, _) => UpdateLocations();
        tracker.StateLoaded += (_, _) => UpdateLocations();
        tracker.BossUpdated += (_, _) => UpdateLocations();
        tracker.MapUpdated += TrackerOnMapUpdated;

        _model.SelectedMap = _model.Maps.Last();
        UpdateMap();
        return _model;
    }

    private void TrackerOnMapUpdated(object? sender, EventArgs e)
    {
        _model.Locations = [];
        var newMap = _model.Maps.FirstOrDefault(x => x.ToString() == tracker.CurrentMap);
        if (newMap != null)
        {
            _model.SelectedMap = newMap;
            UpdateMap();
        }
    }

    public void UpdateLocations(List<TrackerMapLocationViewModel>? locations = null)
    {
        if (_model.SelectedMap == null)
        {
            return;
        }

        locations ??= _model.Locations;

        foreach (var location in locations)
        {
            var region = location.Region;
            var image = "";
            var displayNumber = 0;
            if (location.Type == TrackerMapLocation.MapLocationType.Item)
            {
                var progression = itemService.GetProgression(!(region is HyruleCastle || region.World.Config.KeysanityForRegion(region)));
                var locationStatuses = location.Locations!.Select(x => (Location: x.Location, Status: x.Location.GetStatus(progression))).ToList();

                var clearableLocationsCount = displayNumber = locationStatuses.Count(x => x.Status == LocationStatus.Available);
                var relevantLocationsCount = locationStatuses.Count(x => x.Status == LocationStatus.Relevant);
                //var outOfLogicLocationsCount = Syncer.ShowOutOfLogicLocations ? locationStatuses.Count(x => x.Status == LocationStatus.OutOfLogic) : 0;
                var outOfLogicLocationsCount = false ? locationStatuses.Count(x => x.Status == LocationStatus.OutOfLogic) : 0;
                var unclearedLocationsCount = locationStatuses.Count(x => x.Status != LocationStatus.Cleared);
                var clearedLocationsCount = locationStatuses.Count - unclearedLocationsCount;

                if (clearableLocationsCount > 0 && clearableLocationsCount == unclearedLocationsCount)
                {
                    image = "accessible.png";
                }
                else if (relevantLocationsCount > 0 && relevantLocationsCount + clearableLocationsCount == unclearedLocationsCount)
                {
                    image = "relevant.png";
                }
                else if (relevantLocationsCount > 0 && clearableLocationsCount == 0)
                {
                    image = "partial_relevance.png";
                }
                else if (clearableLocationsCount > 0 && clearableLocationsCount < unclearedLocationsCount)
                {
                    image = "partial.png";
                }
                else if (clearableLocationsCount == 0 && outOfLogicLocationsCount > 0)
                {
                    image = "outoflogic.png";
                }

            }
            else if (location.Type == TrackerMapLocation.MapLocationType.Boss)
            {
                var progression = itemService.GetProgression(region);
                var actualProgression = itemService.GetProgression(false);
                if (location.BossRegion != null && location.BossRegion.Boss.State.Defeated != true && location.BossRegion.CanBeatBoss(progression))
                {
                    image = "boss.png";
                }
                else if (location.RewardRegion != null && location.RewardRegion.Reward.State.Cleared != true)
                {
                    var regionLocations = (IHasLocations)location.Region;

                    // If the player can complete the region with the current actual progression
                    // or if they can access all locations in the dungeon (unless this is Castle Tower
                    // in Keysanity because it doesn't have a location for Aga himself)
                    if (location.RewardRegion.CanComplete(actualProgression)
                        || (regionLocations.Locations.All(x => x.IsAvailable(progression, true))
                            && !(location.Region.Config.ZeldaKeysanity && location.RewardRegion is CastleTower)))
                    {
                        var dungeon = (IDungeon)location.Region;
                        image = dungeon.MarkedReward.GetDescription().ToLowerInvariant() + ".png";
                    }
                }
            }

            if (!string.IsNullOrEmpty(image))
            {
                location.ImagePath = Path.Combine(Sprite.SpritePath, "Maps", image);
                location.IconVisibility = true;
            }
            else
            {
                location.IconVisibility = false;
            }

            if (displayNumber > 1)
            {
                location.NumberImagePath = Path.Combine(
                    Sprite.SpritePath, "Marks", $"{Math.Min(9, displayNumber)}.png");
                location.NumberVisibility = true;
            }
            else
            {
                location.NumberVisibility = false;
            }
        }
    }

    public void UpdateMap()
    {
        var selectedMap = _model.SelectedMap;
        if (selectedMap == null)
        {
            return;
        }

        var locations = _mapLocations[selectedMap];
        UpdateLocations(locations);
        UpdateSize(_model.GridSize, locations);
        _model.Locations = locations;
    }

    public void UpdateSize(Size size, List<TrackerMapLocationViewModel>? locations = null)
    {
        _model.GridSize = size;

        if (_model.SelectedMap == null)
        {
            _model.MapSize = new Size();
            return;
        }

        var currentMap = _model.SelectedMap;
        double imageWidth = currentMap.Width;
        double imageHeight = currentMap.Height;
        var ratio = imageWidth / imageHeight;

        var gridWidth = size.Width;
        var gridHeight = size.Height;

        _model.MapSize = gridWidth / ratio > gridHeight
            ? new Size(gridHeight * ratio, gridHeight)
            : new Size(gridWidth, gridWidth / ratio);

        var offset = (size - _model.MapSize) / 2;

        var mapRatio = _model.MapSize.Width / currentMap.Width;

        locations ??= _model.Locations;
        foreach (var location in locations)
        {
            location.Offset = offset;
            location.Ratio = mapRatio;
        }
    }

    public void Clear(TrackerMapLocationViewModel model)
    {
        if (model.Type == TrackerMapLocation.MapLocationType.Item)
        {
            model.Locations?.Where(x => x.Location.State.Cleared == false)
                .ToList()
                .ForEach(x => tracker.Clear(x.Location));
        }
        else if (model.Type == TrackerMapLocation.MapLocationType.Boss)
        {
            if (model.BossRegion != null)
            {
                tracker.MarkBossAsDefeated(model.BossRegion.Boss);
            }
            else if(model.RewardRegion is IDungeon dungeon)
            {
                tracker.MarkDungeonAsCleared(dungeon);
            }
        }
    }

    public void Clear(TrackerMapSubLocationViewModel model)
    {
        tracker.Clear(model.Location);
    }
}
