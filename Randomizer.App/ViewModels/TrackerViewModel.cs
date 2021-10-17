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
            _tracker.ItemTracked += (_, _) => OnPropertyChanged(nameof(TopLocations));

            World = tracker.World;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public IEnumerable<MarkedLocationViewModel> MarkedLocations
                    => !_isDesign ? _tracker.MarkedLocations.Select(x => new MarkedLocationViewModel(x.Key, x.Value))
                          : GetDummyMarkedLocations();

        public IEnumerable<TopLocationViewModel> TopLocations
        {
            get
            {
                return World.Regions
                    .OrderByDescending(x => x.Locations.Count(x => x.IsAvailable(Progression)))
                    .Take(5)
                    .Select(x => new TopLocationViewModel(x.Locations.Count(x => x.IsAvailable(Progression)), x, Progression));
            }
        }

        protected World World { get; }

        protected Progression Progression => new Progression();

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new(propertyName));

        private IEnumerable<MarkedLocationViewModel> GetDummyMarkedLocations()
        {
            yield return new MarkedLocationViewModel(
                World.LightWorldSouth.Library,
                new ItemData(new("X-Ray Scope"), ItemType.XRay));

            yield return new MarkedLocationViewModel(
                World.LightWorldNorthEast.ZorasDomain.Zora,
                new ItemData(new("Bullshit"), ItemType.Nothing));
        }
    }
}
