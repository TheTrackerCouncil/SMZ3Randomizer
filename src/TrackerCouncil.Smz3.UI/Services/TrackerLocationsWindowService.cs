using System.Collections.Generic;
using System.Linq;
using AvaloniaControls.ControlServices;
using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.Tracking.Services;
using TrackerCouncil.Smz3.UI.ViewModels;

namespace TrackerCouncil.Smz3.UI.Services;

public class TrackerLocationsWindowService(TrackerBase trackerBase, IWorldService worldService, IUIService uiService, IItemService itemService) : ControlService
{
    private TrackerLocationsViewModel _model = new();
    private RegionViewModel? _lastRegion;
    private List<RegionViewModel> _allRegions = [];

    public TrackerLocationsViewModel GetViewModel()
    {
        InitMarkedLocations();
        InitHintTiles();
        InitRegions();

        trackerBase.LocationTracker.LocationMarked += (_, args) =>
        {
            AddUpdateMarkedLocation(args.Location);
        };

        return _model;
    }

    /*public void UpdateModel()
    {
        var markedLocations = new List<MarkedLocationViewModel>();

        var progressionWithoutKeys = itemService.GetProgression(false);
        var progressionWithKeys = itemService.GetProgression(true);

        foreach (var markedLocation in worldService.MarkedLocations())
        {
            var markedItemType = markedLocation.MarkedItem ?? ItemType.Nothing;
            if (markedItemType == ItemType.Nothing) continue;
            var item = itemService.FirstOrDefault(markedItemType);
            if (item == null) continue;
            markedLocations.Add(new MarkedLocationViewModel(markedLocation, item, uiService.GetSpritePath(item),
                markedLocation.IsAvailable(progressionWithoutKeys)));
        }

        //_model.MarkedLocations = markedLocations;

        _model.HintTiles = worldService.ViewedHintTiles
            .Where(x => x.Locations?.Count() > 1)
            .Select(x => new HintTileViewModel(x))
            .ToList();


    }*/

    public void InitMarkedLocations()
    {
        foreach (var markedLocation in worldService.MarkedLocations())
        {
            AddUpdateMarkedLocation(markedLocation);
        }
    }

    public void AddUpdateMarkedLocation(Location location)
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

    public void InitHintTiles()
    {
        _model.HintTiles = worldService.World.HintTiles
            .Where(x => x.Locations?.Count() > 1)
            .Select(x => new HintTileViewModel(x))
            .ToList();
    }

    public void InitRegions()
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
                _model.Regions = _allRegions.Where(x => x.VisibleLocations > 0).OrderByDescending(x => x.SortOrder)
                    .ThenByDescending(x => x.VisibleLocations)
                    .ToList();
            };
        }

        _allRegions = regionModels;
        _model.Regions = _allRegions.Where(x => x.VisibleLocations > 0).OrderByDescending(x => x.SortOrder)
            .ThenByDescending(x => x.VisibleLocations).ToList();;
        _lastRegion = _model.Regions.First();
        _lastRegion.SortOrder = 1;
    }

    public void ClearLocation(LocationViewModel model)
    {
        if (model.Location == null)
        {
            return;
        }
        trackerBase.LocationTracker.Clear(model.Location);
    }
}
