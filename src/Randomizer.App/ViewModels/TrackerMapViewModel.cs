using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Randomizer.SMZ3.Tracking.Configuration.ConfigTypes;

namespace Randomizer.App.ViewModels
{
    /// <summary>
    /// ViewModel to use for displaying the Tracker Map Window
    /// </summary>
    public class TrackerMapViewModel : INotifyPropertyChanged
    {
        private readonly bool _isDesign;

        /// <summary>
        /// The current map the user has selected
        /// </summary>
        private TrackerMap _currentMap;

        /// <summary>
        /// The size of the parent grid of the canvas to use for resizing the
        /// canvas
        /// </summary>
        private Size _gridSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="TrackerMap"/> class
        /// with data to display a specific location on the map and its progress
        /// </summary>
        public TrackerMapViewModel()
        {
            _isDesign = DesignerProperties.GetIsInDesignMode(new DependencyObject());
        }

        /// <summary>
        /// Event for when one of the values has been updated to notify the
        /// window to refresh
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// List of all map names for displaying in the dropdown
        /// </summary>
        public List<string> MapNames { get; set; }

        /// <summary>
        /// A reference to the original <see cref="TrackerViewModel"/> object to
        /// keep synced up
        /// </summary>
        public TrackerViewModel TrackerViewModel { get; set; }

        /// <summary>
        /// The current map the user has selected
        /// </summary>
        public TrackerMap CurrentMap
        {
            get => _currentMap;
            set
            {
                _currentMap = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// The size of the parent grid of the canvas to use for resizing the
        /// canvas
        /// </summary>
        public Size GridSize
        {
            get => _gridSize;
            set
            {
                _gridSize = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Calculated size of the map canvas to maintain the image aspect ratio
        /// </summary>
        public Size MapSize
        {
            get
            {
                if (_isDesign)
                    return new Size(400, 400);

                if (CurrentMap == null)
                    return new Size(0, 0);

                double imageWidth = CurrentMap.Width;
                double imageHeight = CurrentMap.Height;
                double ratio = imageWidth / imageHeight;

                double gridWidth = GridSize.Width;
                double gridHeight = GridSize.Height;

                if (gridWidth / ratio > gridHeight)
                {
                    return new Size(gridHeight * ratio, gridHeight);
                }
                else
                {
                    return new Size(gridWidth, gridWidth / ratio);
                }
            }
        }

        /// <summary>
        /// The background image to display in the canvas of the map
        /// </summary>
        public ImageSource CanvasImage
        {
            get
            {
                if (_isDesign || CurrentMap == null)
                {
                    return new BitmapImage(new Uri(System.IO.Path.Combine(
                    System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    "Sprites", "Maps", "lttp_lightworld.png")));
                }

                return new BitmapImage(new Uri(System.IO.Path.Combine(
                    System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    "Sprites", "Maps", CurrentMap.Image)));
            }
        }

        /// <summary>
        /// TrackerLocationSyncer used to keep items synced between both
        /// locations and map windows
        /// </summary>
        public TrackerLocationSyncer Syncer { get; set; }

        /// <summary>
        /// A list of all tracker map location view models to display on the map
        /// </summary>
        public List<TrackerMapLocationViewModel> TrackerMapLocations
        {
            get
            {
                if (CurrentMap == null) return new List<TrackerMapLocationViewModel>();

                // This is used to determine if there are invalid locations on
                // the map
                if (Debugger.IsAttached)
                {
                    var test = _currentMap.FullLocations
                        .Where(mapLoc => Syncer.AllLocations.Where(loc => mapLoc.MatchesSMZ3Location(loc)).Count() == 0)
                        .Select(mapLoc => mapLoc.Name)
                        .ToList();

                    if (test.Count > 0)
                    {
                        Console.WriteLine(test);
                    }
                }

                return _currentMap.FullLocations
                    .Where(x => x.Type != TrackerMapLocation.MapLocationType.SMDoor || Syncer.World.Config.MetroidKeysanity)
                    .Select(mapLoc => new TrackerMapLocationViewModel(mapLoc,
                        Syncer,
                        MapSize.Width / CurrentMap.Width))
                    .ToList();
            }
        }

        /// <summary>
        /// Call to execute the PropertyChanged event to notify the window to
        /// refresh
        /// </summary>
        public void OnPropertyChanged()
        {
            PropertyChanged?.Invoke(this, new(""));
        }
    }
}
