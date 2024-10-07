using System.Collections.Generic;
using System.Linq;
using AvaloniaControls.ControlServices;
using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.Tracking.Services;
using TrackerCouncil.Smz3.UI.ViewModels;

namespace TrackerCouncil.Smz3.UI.Services;

public class TrackerLocationsWindowService(TrackerBase trackerBase, IWorldService worldService, IUIService uiService, IItemService itemService) : ControlService
{
    private TrackerLocationsViewModel _model = new();

    public TrackerLocationsViewModel GetViewModel()
    {
        UpdateModel();

        /*trackerBase.MarkedLocationsUpdated += (_, _) => UpdateModel();
        trackerBase.LocationCleared += (_, _) => UpdateModel();
        trackerBase.DungeonUpdated += (_, _) => UpdateModel();
        trackerBase.ItemTracked += (_, _) => UpdateModel();
        trackerBase.ActionUndone += (_, _) => UpdateModel();
        trackerBase.StateLoaded += (_, _) => UpdateModel();
        trackerBase.BossUpdated += (_, _) => UpdateModel();
        trackerBase.HintTileUpdated += (_, _) => UpdateModel();*/

        return _model;
    }

    public void UpdateModel()
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

        _model.MarkedLocations = markedLocations;

        _model.HintTiles = worldService.ViewedHintTiles
            .Where(x => x.Locations?.Count() > 1)
            .Select(x => new HintTileViewModel(x))
            .ToList();

        var locations = worldService.Locations(unclearedOnly: true, outOfLogic: _model.ShowOutOfLogic, assumeKeys: true,
            sortByTopRegion: true, regionFilter: _model.Filter).ToList();

        _model.Regions = locations.Select(x => x.Region).Distinct().Select(region => new RegionViewModel()
        {
            RegionName = region.ToString(),
            Locations = locations.Where(loc => loc.Region == region)
                .Select(loc => new LocationViewModel(loc, loc.IsAvailable(progressionWithoutKeys),
                    loc.IsAvailable(progressionWithKeys)))
                .ToList()
        }).ToList();
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
