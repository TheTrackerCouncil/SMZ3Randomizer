using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;

using Randomizer.SMZ3;
using Randomizer.SMZ3.Tracking;

namespace Randomizer.App.ViewModels
{
    public class TrackerViewModel : INotifyPropertyChanged
    {
        private readonly Tracker _tracker;
        private bool _isDesign;
        private LocationFilter _filter;
        private Region _stickyRegion = null;
        private bool _showOutOfLogicLocations;

        public TrackerViewModel()
        {
            _isDesign = DesignerProperties.GetIsInDesignMode(new DependencyObject());
            World = new World(new Config(), "", 0, "");
        }

        public TrackerViewModel(Tracker tracker)
        {
            _tracker = tracker;
            _tracker.MarkedLocationsUpdated += (_, _) => OnPropertyChanged(nameof(MarkedLocations));
            _tracker.LocationCleared += (_, e) =>
            {
                _stickyRegion = e.Location.Region;
                OnPropertyChanged(nameof(TopLocations));
                OnPropertyChanged(nameof(MarkedLocations));
            };
            _tracker.DungeonUpdated += (_, _) =>
            {
                OnPropertyChanged(nameof(TopLocations));
                OnPropertyChanged(nameof(MarkedLocations));
            };
            _tracker.ItemTracked += (_, _) =>
            {
                OnPropertyChanged(nameof(TopLocations));
                OnPropertyChanged(nameof(MarkedLocations));
            };
            _tracker.ActionUndone += (_, _) =>
            {
                OnPropertyChanged(nameof(TopLocations));
                OnPropertyChanged(nameof(MarkedLocations));
            };

            World = tracker.World;
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
            get => _showOutOfLogicLocations;
            set
            {
                if (value != _showOutOfLogicLocations)
                {
                    _showOutOfLogicLocations = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(TopLocations));
                }
            }
        }

        public IEnumerable<MarkedLocationViewModel> MarkedLocations
        {
            get
            {
                if (_isDesign)
                    return GetDummyMarkedLocations();

                return _tracker.MarkedLocations.Select(x =>
                {
                    var location = World.Locations.Single(location => location.Id == x.Key);
                    return new MarkedLocationViewModel(location, x.Value, Progression);
                });
            }
        }

        public IEnumerable<LocationViewModel> TopLocations
        {
            get
            {
                var regions = World.Regions
                    .Where(RegionFilterCondition)
                    .OrderByDescending(x => x.Locations.Count(x => !x.Cleared && x.IsAvailable(Progression)))
                    .ToList();

                if (_stickyRegion != null && regions.Contains(_stickyRegion))
                    regions.MoveToTop(x => x == _stickyRegion);

                return regions.SelectMany(x => x.Locations)
                    .Where(ShouldShowLocation)
                    .Select(x => new LocationViewModel(x,
                        Progression,
                        ProgressionWithKeys,
                        onClear: () => OnPropertyChanged(nameof(TopLocations))))
                    .ToImmutableList();

                bool ShouldShowLocation(Location x) => !x.Cleared && (x.IsAvailable(Progression)
                                                                      || x.IsAvailable(ProgressionWithKeys)
                                                                      || ShowOutOfLogicLocations);
            }
        }

        protected World World { get; }

        protected Progression Progression => _tracker?.GetProgression() ?? new();

        protected Progression ProgressionWithKeys => _tracker?.GetProgression(true) ?? new();

        private Func<Region, bool> RegionFilterCondition => Filter switch
        {
            LocationFilter.None => _ => true,
            LocationFilter.ZeldaOnly => x => x is Z3Region,
            LocationFilter.MetroidOnly => x => x is SMRegion,
            _ => throw new InvalidEnumArgumentException(nameof(Filter), (int)Filter, typeof(LocationFilter)),
        };

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
                    => PropertyChanged?.Invoke(this, new(propertyName));

        private IEnumerable<MarkedLocationViewModel> GetDummyMarkedLocations()
        {
            yield return new MarkedLocationViewModel(
                World.LightWorldSouth.Library,
                new ItemData(new("X-Ray Scope"), ItemType.XRay),
                Progression);

            yield return new MarkedLocationViewModel(
                World.LightWorldNorthEast.ZorasDomain.Zora,
                new ItemData(new("Bullshit"), ItemType.Nothing),
                Progression);
        }
    }
}
