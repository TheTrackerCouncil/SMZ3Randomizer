using System.Collections.Generic;
using System.Linq;
using Avalonia.Threading;
using AvaloniaControls.ControlServices;
using AvaloniaControls.Services;
using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.Tracking.Services;
using TrackerCouncil.Smz3.UI.ViewModels;

namespace TrackerCouncil.Smz3.UI.Services;

public class TrackerLocationsWindowService(TrackerBase trackerBase, IWorldService worldService, IUIService uiService, IItemService itemService) : ControlService
{
    private readonly TrackerLocationsViewModel _model = new();
    private RegionViewModel? _lastRegion;
    private List<RegionViewModel> _allRegions = [];

    public TrackerLocationsViewModel GetViewModel()
    {
        ITaskService.Run(() =>
        {
            InitMarkedLocations();
            InitHintTiles();
            InitRegions();

            trackerBase.LocationTracker.LocationMarked += (_, args) =>
            {
                AddUpdateMarkedLocation(args.Location);
            };

            _model.FinishedLoading = true;
        });

        return _model;
    }

    public void UpdateShowOutOfLogic(bool showOutOfLogic)
    {
        // Because the map update is snappy while this is slow, run this in a separate
        // thread to avoid locking up the map
        ITaskService.Run(() =>
        {
            _model.ShowOutOfLogic = showOutOfLogic;

            foreach (var region in _allRegions)
            {
                foreach (var location in region.Locations)
                {
                    location.ShowOutOfLogic = showOutOfLogic;
                }

                region.ShowOutOfLogic = showOutOfLogic;
                region.UpdateLocationCount();
                region.SortLocations();
            }

            ShowSortedRegions();
        });

    }

    public void UpdateFilter(RegionFilter filter)
    {
        _model.Filter = filter;

        foreach (var region in _allRegions)
        {
            region.MatchesFilter = region.Region?.MatchesFilter(filter) == true;
        }
    }

    public void ClearLocation(LocationViewModel model)
    {
        if (model.Location == null)
        {
            return;
        }
        trackerBase.LocationTracker.Clear(model.Location);
    }

    private void InitMarkedLocations()
    {
        foreach (var markedLocation in worldService.MarkedLocations())
        {
            AddUpdateMarkedLocation(markedLocation);
        }
    }

    private void AddUpdateMarkedLocation(Location location)
    {
        if (location.MarkedItem == null)
        {
            var previousModel = _model.MarkedLocations.FirstOrDefault(x => x.Location == location);
            if (previousModel != null)
            {
                location.AccessibilityUpdated -= previousModel.LocationOnAccessibilityUpdated;
                _model.MarkedLocations.Remove(previousModel);
            }
        }
        else
        {
            if (location.MarkedItem == ItemType.Nothing) return;
            var item = itemService.FirstOrDefault(location.MarkedItem.Value);
            if (item == null) return;

            var previousModel = _model.MarkedLocations.FirstOrDefault(x => x.Location == location);
            if (previousModel != null)
            {
                location.AccessibilityUpdated -= previousModel.LocationOnAccessibilityUpdated;
                _model.MarkedLocations.Remove(previousModel);
            }

            var newModel = new MarkedLocationViewModel(location, item, uiService.GetSpritePath(item));
            location.AccessibilityUpdated += newModel.LocationOnAccessibilityUpdated;
            _model.MarkedLocations.Add(newModel);
        }
    }

    private void InitHintTiles()
    {
        var world = worldService.World;
        var hintTiles = new List<HintTileViewModel>();

        foreach (var worldHintTile in world.HintTiles.Where(x => x.Locations?.Count() > 1))
        {
            var locationIds = worldHintTile.Locations!.ToHashSet();
            var locations = world.Locations.Where(x => locationIds.Contains(x.Id));
            hintTiles.Add(new HintTileViewModel(worldHintTile, locations));
        }

        _model.HintTiles = hintTiles;
    }

    private void InitRegions()
    {
        var regions = worldService.World.Regions;
        var regionModels = new List<RegionViewModel>();

        foreach (var region in regions)
        {
            var regionModel = new RegionViewModel(region);
            regionModels.Add(regionModel);
            regionModel.RegionUpdated += (_, _) =>
            {
                if (_lastRegion == regionModel) return;

                regionModel.SortOrder = 1;
                if (_lastRegion != null)
                {
                    _lastRegion.SortOrder = 0;
                }

                _lastRegion = regionModel;
                ShowSortedRegions();
            };
        }

        _allRegions = regionModels;
        ShowSortedRegions();
        _lastRegion = _model.Regions.First();
        _lastRegion.SortOrder = 1;
    }

    private void ShowSortedRegions()
    {
        _model.Regions = _allRegions.Where(x => x.VisibleLocations > 0).OrderByDescending(x => x.SortOrder)
            .ThenByDescending(x => x.InLogicLocationCount)
            .ToList();
    }
}
