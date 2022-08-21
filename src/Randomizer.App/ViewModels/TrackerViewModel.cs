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
using Randomizer.SMZ3.Tracking.Configuration;
using Randomizer.SMZ3.Tracking.Configuration.ConfigTypes;
using Randomizer.SMZ3.Tracking.Services;

namespace Randomizer.App.ViewModels
{
    public class TrackerViewModel : INotifyPropertyChanged
    {
        private readonly IUIService _uiService;
        private bool _isDesign;
        private LocationFilter _filter;
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

        public LocationFilter Filter
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

                return _syncer.MarkedLocations.Select(x =>
                {
                    var location = _syncer.AllLocations.Single(location => location.Id == x.Key);
                    return new MarkedLocationViewModel(location, x.Value, _uiService.GetSpritePath(x.Value), _syncer);
                });
            }
        }

        public IEnumerable<LocationViewModel> TopLocations
        {
            get
            {
                return _syncer.GetTopLocations(Filter)
                    .Select(x => new LocationViewModel(x, _syncer))
                    .ToImmutableList();
            }
        }


        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
                    => PropertyChanged?.Invoke(this, new(propertyName));

        private IEnumerable<MarkedLocationViewModel> GetDummyMarkedLocations()
        {
            var item = new ItemData(new("X-Ray Scope"), ItemType.XRay, null);
            yield return new MarkedLocationViewModel(
                _syncer.World.LightWorldSouth.Library,
                item,
                null,
                _syncer);

            item = new ItemData(new("Bullshit"), ItemType.Nothing, null);
            yield return new MarkedLocationViewModel(
                _syncer.World.LightWorldNorthEast.ZorasDomain.Zora,
                item,
                null,
                _syncer);
        }
    }
}
