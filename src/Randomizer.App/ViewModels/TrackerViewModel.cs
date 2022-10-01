using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using Randomizer.Shared;
using Randomizer.SMZ3;
using Randomizer.SMZ3.Tracking;
using Randomizer.Data.Configuration;
using Randomizer.Data.Configuration.ConfigTypes;
using Randomizer.SMZ3.Tracking.Services;
using Randomizer.Data.WorldData;
using Randomizer.Shared.Enums;

namespace Randomizer.App.ViewModels
{
    public class TrackerViewModel : INotifyPropertyChanged
    {
        private readonly IUIService _uiService;
        private bool _isDesign;
        private RegionFilter _filter;
        private TrackerLocationSyncer _syncer;

        public TrackerViewModel(IUIService uiService)
        {
            _isDesign = DesignerProperties.GetIsInDesignMode(new DependencyObject());
            _syncer = new TrackerLocationSyncer();
            _uiService = uiService;
        }

        public TrackerViewModel(TrackerLocationSyncer syncer, IUIService uiService)
        {
            _syncer = syncer;
            _syncer.TrackedLocationUpdated += (_, _) => OnPropertyChanged(nameof(TopLocations));
            _syncer.MarkedLocationUpdated += (_, _) => OnPropertyChanged(nameof(MarkedLocations));
            _uiService = uiService;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public RegionFilter Filter
        {
            get => _filter;
            set
            {
                if (value != _filter)
                {
                    _filter = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(TopLocations));
                }
            }
        }

        public bool ShowOutOfLogicLocations
        {
            get => _syncer.ShowOutOfLogicLocations;
            set => _syncer.ShowOutOfLogicLocations = value;
        }

        public IEnumerable<MarkedLocationViewModel> MarkedLocations
        {
            get
            {
                if (_isDesign)
                    return GetDummyMarkedLocations();

                return _syncer.WorldService.MarkedLocations()
                    .Select(loc => (location: loc, item: _syncer.Tracker.ItemService.FirstOrDefault(loc.State.MarkedItem.Value)))
                    .Select(loc =>
                    {
                        return new MarkedLocationViewModel(loc.location, loc.item, _uiService.GetSpritePath(loc.item), _syncer);
                    });
            }
        }

        public IEnumerable<LocationViewModel> TopLocations
        {
            get
            {
                return _syncer.WorldService.Locations(unclearedOnly: true, outOfLogic: ShowOutOfLogicLocations, assumeKeys: true, sortByTopRegion: true, regionFilter: Filter)
                    .Select(x => new LocationViewModel(x, _syncer))
                    .ToImmutableList();
            }
        }


        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
                    => PropertyChanged?.Invoke(this, new(propertyName));

        private IEnumerable<MarkedLocationViewModel> GetDummyMarkedLocations()
        {
            var item = new Item(ItemType.XRay, null, "X-Ray Scope");
            yield return new MarkedLocationViewModel(
                _syncer.World.LightWorldSouth.Library,
                item,
                null,
                _syncer);

            item = new Item(ItemType.XRay, null, "Bow");
            yield return new MarkedLocationViewModel(
                _syncer.World.LightWorldNorthEast.ZorasDomain.Zora,
                item,
                null,
                _syncer);
        }
    }
}
