using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia;
using AvaloniaControls;
using AvaloniaControls.ControlServices;
using AvaloniaControls.Services;
using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Data.Configuration.ConfigFiles;
using TrackerCouncil.Smz3.Data.Configuration.ConfigTypes;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.Data.WorldData.Regions;
using TrackerCouncil.Smz3.SeedGenerator.Contracts;
using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.Tracking.Services;
using TrackerCouncil.Smz3.UI.ViewModels;

namespace TrackerCouncil.Smz3.UI.Services;

public class TrackerMapWindowService(
    TrackerBase tracker,
    TrackerMapConfig trackerMapConfig,
    IWorldAccessor worldAccessor,
    IWorldQueryService worldQueryService) : ControlService
{
    private readonly TrackerMapWindowViewModel _model = new();
    private readonly Dictionary<TrackerMap, List<TrackerMapLocationViewModel>> _mapLocations = new();
    private string _markedImageGoodPath = "";
    private string _markedImageUselessPath = "";

    public TrackerMapWindowViewModel GetViewModel()
    {
        _model.Maps = trackerMapConfig.Maps.ToList();

        // The map initializes really slowly, so actually initialize the data in a separate thread
        ITaskService.Run(InitViewModelData);

        return _model;
    }

    private void InitViewModelData()
    {
        _markedImageGoodPath = Path.Combine(Sprite.SpritePath, "Maps", "marked_good.png");
        _markedImageUselessPath = Path.Combine(Sprite.SpritePath, "Maps", "marked_useless.png");

        var allLocations = worldAccessor.World.Locations.ToList();

        // To make querying easier for the viewmodel, compile a list of all
        // locations on each map by combining all of their regions, making
        // any location adjustments needed
        foreach (var map in _model.Maps)
        {
            var locationModels = new List<TrackerMapLocationViewModel>();

            foreach (var mapLocation in map.Regions)
            {
                var mapRegion = trackerMapConfig.Regions.First(region => mapLocation.Name == region.Name);

                if (mapLocation.Type == TrackerMapLocation.MapLocationType.Item)
                {
                    locationModels.AddRange(GetItemLocationModels(mapRegion, mapLocation, allLocations));
                }

                if (mapRegion is { BossX: not null, BossY: not null })
                {
                    locationModels.Add(GetBossLocationModel(mapRegion, mapLocation));
                }

                if (mapRegion.Doors != null && worldAccessor.World.Config.MetroidKeysanity)
                {
                    locationModels.AddRange(GetDoorLocationModels(mapRegion, mapLocation));
                }
            }

            _mapLocations[map] = locationModels;
        }

        tracker.GameStateTracker.MapUpdated += TrackerOnMapUpdated;

        _model.SelectedMap = _model.Maps.Last();
        UpdateMap();

        _model.FinishedLoading = true;
    }

    private List<TrackerMapLocationViewModel> GetItemLocationModels(TrackerMapRegion mapRegion, TrackerMapLocation mapLocation, List<Location> allLocations)
    {
        var worldRegion = worldQueryService.Region(mapRegion.TypeName) ?? throw new InvalidOperationException();
        var regionLocations = allLocations.Where(loc => mapRegion.TypeName == loc.Region.GetType().FullName).ToList();
        var toReturn = new List<TrackerMapLocationViewModel>();

        foreach (var room in mapRegion.Rooms!)
        {
            var roomLocations = regionLocations.Where(loc =>
                room.Name == loc.Name || room.Name == loc.Room?.Name || mapRegion.Name == room.Name).ToList();

            var roomModel = new TrackerMapLocationViewModel(mapRegion, mapLocation, room, worldRegion, roomLocations);

            foreach (var location in roomLocations)
            {
                location.AccessibilityUpdated += (_, _) => UpdateItemLocationModel(roomModel, roomLocations);
            }

            UpdateItemLocationModel(roomModel, roomLocations);

            toReturn.Add(roomModel);
        }

        return toReturn;
    }

    private void UpdateItemLocationModel(TrackerMapLocationViewModel model, List<Location>? locations)
    {
        var region = model.Region;
        var image = "";
        var displayNumber = 0;

        locations ??= region.Locations.Where(loc =>
            model.Name == loc.Name || model.Name == loc.Room?.Name || region.Name == model.Name).ToList();

        var locationStatuses = locations
            .Select(x => (Location: x, Status: x.GetKeysanityAdjustedAccessibility())).ToList();

        var clearableLocationsCount = displayNumber = locationStatuses.Count(x => x.Status == Accessibility.Available);
        var relevantLocationsCount = locationStatuses.Count(x => x.Status == Accessibility.Relevant);
        var outOfLogicLocationsCount = locationStatuses.Count(x => x.Status == Accessibility.OutOfLogic);
        var unclearedLocationsCount = locationStatuses.Count(x => x.Status != Accessibility.Cleared);

        if (clearableLocationsCount > 0 && clearableLocationsCount == unclearedLocationsCount)
        {
            model.IsInLogic = true;
            image = "accessible.png";
        }
        else if (relevantLocationsCount > 0 && relevantLocationsCount + clearableLocationsCount == unclearedLocationsCount)
        {
            model.IsInLogic = true;
            image = "relevant.png";
        }
        else if (relevantLocationsCount > 0 && clearableLocationsCount == 0)
        {
            model.IsInLogic = true;
            image = "partial_relevance.png";
        }
        else if (clearableLocationsCount > 0 && clearableLocationsCount < unclearedLocationsCount)
        {
            model.IsInLogic = true;
            image = "partial.png";
        }
        else if (clearableLocationsCount == 0 && outOfLogicLocationsCount > 0)
        {
            model.IsInLogic = false;
            image = "outoflogic.png";
        }

        // If there are any valid locations, see if anything was marked
        if (clearableLocationsCount > 0 || relevantLocationsCount > 0)
        {
            var usefulness = locationStatuses
                .Where(x => x.Status is Accessibility.Available or Accessibility.Relevant)
                .Max(x => x.Location.MarkedUsefulness);

            if (usefulness is null)
            {
                model.MarkedVisibility = false;
            }
            else if (usefulness == LocationUsefulness.Useless)
            {
                model.MarkedImagePath = _markedImageUselessPath;
                model.MarkedVisibility = true;
            }
            else
            {
                model.MarkedImagePath = _markedImageGoodPath;
                model.MarkedVisibility = true;
            }
        }
        else
        {
            model.MarkedVisibility = false;
        }

        UpdateLocationModel(model, image, displayNumber);
    }

    private TrackerMapLocationViewModel GetBossLocationModel(TrackerMapRegion mapRegion, TrackerMapLocation mapLocation)
    {
        var worldRegion = worldQueryService.Region(mapRegion.TypeName) ?? throw new InvalidOperationException();
        var model = new TrackerMapLocationViewModel(mapRegion, mapLocation, worldRegion);

        if (worldRegion is IHasBoss bossRegion)
        {
            bossRegion.Boss.UpdatedAccessibility += (_, _) => UpdateBossLocationModel(model);
            bossRegion.Boss.UpdatedBossState += (_, _) => UpdateBossLocationModel(model);
        }

        if (worldRegion is IHasReward rewardRegion)
        {
            rewardRegion.Reward.UpdatedAccessibility += (_, _) => UpdateBossLocationModel(model);
            rewardRegion.Reward.UpdatedRewardState += (_, _) => UpdateBossLocationModel(model);
        }

        UpdateBossLocationModel(model);
        return model;
    }

    private void UpdateBossLocationModel(TrackerMapLocationViewModel location)
    {
        var image = "";

        if (location.RewardRegion is { HasReceivedReward: false } rewardRegion && rewardRegion.GetKeysanityAdjustedBossAccessibility() == Accessibility.Available)
        {
            image = rewardRegion.MarkedReward.GetDescription().ToLowerInvariant() + ".png";
            location.IsInLogic = true;
        }
        else if (location.BossRegion is { BossDefeated: false } && location.BossRegion.GetKeysanityAdjustedBossAccessibility() == Accessibility.Available)
        {
            image = "boss.png";
            location.IsInLogic = true;
        }
        else
        {
            location.IsInLogic = false;
        }

        UpdateLocationModel(location, image);
    }

    private List<TrackerMapLocationViewModel> GetDoorLocationModels(TrackerMapRegion mapRegion, TrackerMapLocation mapLocation)
    {
        var doors = mapRegion.Doors ?? throw new InvalidOperationException();
        var worldRegion = worldQueryService.Region(mapRegion.TypeName) ?? throw new InvalidOperationException();
        var toReturn = new List<TrackerMapLocationViewModel>();

        foreach (var door in doors)
        {
            var model = new TrackerMapLocationViewModel(mapRegion, mapLocation, worldRegion, door);
            var item = worldQueryService.FirstOrDefault(door.Item) ?? throw new InvalidOperationException();
            item.UpdatedItemState += (_, _) => UpdateDoorLocationModel(model, item);
            toReturn.Add(model);
            UpdateDoorLocationModel(model, item);
        }

        return toReturn;
    }

    private void UpdateDoorLocationModel(TrackerMapLocationViewModel location, Item? item)
    {
        var image = "";
        item ??= worldQueryService.FirstOrDefault(location.Name);
        if (item?.TrackingState > 0)
        {
            image = "";
        }
        else if (item?.Type.IsInCategory(ItemCategory.KeycardL1) == true)
        {
            image = "door1.png";
        }
        else if (item?.Type.IsInCategory(ItemCategory.KeycardL2) == true)
        {
            image = "door2.png";
        }
        else if (item?.Type.IsInCategory(ItemCategory.KeycardBoss) == true)
        {
            image = "doorb.png";
        }

        UpdateLocationModel(location, image);
    }

    private void UpdateLocationModel(TrackerMapLocationViewModel location, string image, int displayNumber = 0)
    {
        if (!string.IsNullOrEmpty(image))
        {
            location.ImagePath = Path.Combine(Sprite.SpritePath, "Maps", image);
            location.HasImage = true;
        }
        else
        {
            location.HasImage = false;
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

    private void TrackerOnMapUpdated(object? sender, EventArgs e)
    {
        if (_model.SelectedMap?.ToString() == tracker.GameStateTracker.CurrentMap)
        {
            return;
        }

        _model.Locations = [];
        var newMap = _model.Maps.FirstOrDefault(x => x.ToString() == tracker.GameStateTracker.CurrentMap);
        if (newMap != null)
        {
            _model.SelectedMap = newMap;
            UpdateMap();
        }
    }

    public void UpdateOutOfLogic()
    {
        foreach (var location in _mapLocations.Values.SelectMany(x => x))
        {
            location.ShowOutOfLogic = _model.ShowOutOfLogicLocations;
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
            model.Locations?.Where(x => x.Location.Cleared == false)
                .ToList()
                .ForEach(x => tracker.LocationTracker.Clear(x.Location));
        }
        else if (model.Type == TrackerMapLocation.MapLocationType.Boss)
        {
            if (model.BossRegion != null)
            {
                tracker.BossTracker.MarkRegionBossAsDefeated(model.BossRegion, admittedGuilt: true);
            }
            else if(model.RewardRegion != null)
            {
                tracker.RewardTracker.GiveAreaReward(model.RewardRegion, false, true);
            }
        }
    }

    public void Clear(TrackerMapSubLocationViewModel model)
    {
        tracker.LocationTracker.Clear(model.Location);
    }
}
