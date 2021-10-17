using System;
using System.Collections.Generic;
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

        public TrackerViewModel()
        {
            _isDesign = DesignerProperties.GetIsInDesignMode(new DependencyObject());
            World = new World(new Config(), "", 0, "");
        }

        public TrackerViewModel(Tracker tracker)
        {
            _tracker = tracker;
            _tracker.MarkedLocationsUpdated += (_, _) => OnPropertyChanged(nameof(MarkedLocations));
            _tracker.ItemTracked += (_, _) =>
            {
                OnPropertyChanged(nameof(TopLocations));
                OnPropertyChanged(nameof(MarkedLocations));
            };

            World = tracker.World;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public IEnumerable<MarkedLocationViewModel> MarkedLocations
                    => !_isDesign ? _tracker.MarkedLocations.Select(x => new MarkedLocationViewModel(x.Key, x.Value, Progression))
                          : GetDummyMarkedLocations();

        public IEnumerable<LocationViewModel> TopLocations
        {
            get
            {
                return World.Regions
                    .OrderByDescending(x => x.Locations.Count(x => x.IsAvailable(Progression) && !x.Cleared))
                    .Take(3)
                    .SelectMany(x => x.Locations)
                    .Where(x => x.IsAvailable(Progression) && !x.Cleared)
                    .Select(x => new LocationViewModel(x, () => OnPropertyChanged(nameof(TopLocations))));
            }
        }

        protected World World { get; }

        protected Progression Progression => _tracker?.GetProgression() ?? new();

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
