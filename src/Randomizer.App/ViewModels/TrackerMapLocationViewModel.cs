using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Randomizer.SMZ3;
using Randomizer.SMZ3.Regions;
using Randomizer.SMZ3.Regions.Zelda;
using Randomizer.SMZ3.Tracking.Configuration;

namespace Randomizer.App.ViewModels
{
    /// <summary>
    /// ViewModel for displaying a specific location on the map and its progress
    /// </summary>
    public class TrackerMapLocationViewModel
    {
        private static readonly Style s_contextMenuStyle = Application.Current.FindResource("DarkContextMenu") as Style;

        /// <summary>
        /// Initializes a new instance of the <see cref="TrackerMap"/> class
        /// with data to display a specific location on the map and its progress
        /// </summary>
        /// <param name="mapLocation">
        /// The TrackerMapLocation with details of where to place this location
        /// on the map
        /// </param>
        /// <param name="syncer">
        /// The LocationSyncer for keeping in sync with the data in the tracker
        /// </param>
        /// <param name="scaledRatio">
        /// The ratio in which the canvas has been scaled to fit in the grid
        /// </param>
        public TrackerMapLocationViewModel(TrackerMapLocation mapLocation, TrackerLocationSyncer syncer, double scaledRatio)
        {
            Size = 20;
            X = (mapLocation.X * scaledRatio) - (Size / 2);
            Y = (mapLocation.Y * scaledRatio) - (Size / 2);
            Syncer = syncer ?? throw new ArgumentNullException(nameof(syncer));
            Region = Syncer.World.Regions.First(x => x.GetType().FullName == mapLocation.RegionTypeName);

            // If no location was specified, it's a boss or dungeon
            if (mapLocation.Name == null)
            {
                if (Region is Z3Region)
                {
                    Dungeon = Syncer.Tracker.WorldInfo.Dungeons.First(x => x.TypeName == mapLocation.RegionTypeName);
                    Name = Dungeon.Reward.GetDescription();
                    Y -= 22;
                }
                else if (Region is SMRegion)
                {
                    Boss = Syncer.Tracker.WorldInfo.Bosses.First(x => x.Reward == ((IHasReward)Region).Reward);
                    Name = Boss.ToString();
                }
            }
            // Else figure out the current status of all of the locations
            else
            {
                Name = Syncer.GetName(mapLocation);
                Locations = Syncer.AllLocations.Where(loc => mapLocation.MatchesSMZ3Location(loc)).ToList();

                var progression = Region is HyruleCastle || Region.World.Config.Keysanity ? syncer.Progression : syncer.ProgressionWithKeys;
                var statuses = Locations.Select(x => x.GetStatus(progression));

                ClearableLocationsCount = statuses.Count(x => x == Shared.Enums.LocationStatus.Available);
                RelevantLocationsCount = statuses.Count(x => x == Shared.Enums.LocationStatus.Relevant);
                OutOfLogicLocationsCount = Syncer.ShowOutOfLogicLocations ? statuses.Count(x => x == Shared.Enums.LocationStatus.OutOfLogic) : 0;
                UnclearedLocationsCount = statuses.Count(x => x != Shared.Enums.LocationStatus.Cleared);
                ClearedLocationsCount = statuses.Count() - UnclearedLocationsCount;
            }

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TrackerMap"/> class
        /// with data to display a specific map location in the sub menu
        /// </summary>
        /// <param name="location"></param>
        public TrackerMapLocationViewModel(Location location, TrackerLocationSyncer syncer)
        {
            Locations = new List<Location>() { location };
            Syncer = syncer;
            Name = $"Clear {Syncer.GetName(location)}";
        }

        /// <summary>
        /// The list of SMZ3 randomizer locations to look at to determine
        /// progress
        /// </summary>
        public List<Location> Locations { get; set; }

        /// <summary>
        /// The list of locations underneath this one for the right click menu
        /// </summary>
        public List<TrackerMapLocationViewModel> SubLocationModels
            => Locations?.Where(x => Syncer.IsLocationClearable(x))
                        .Select(x => new TrackerMapLocationViewModel(x, Syncer))
                        .ToList() ?? new();

        /// <summary>
        /// The X value of where to display this location on the map
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// The Y value of where to display this location on the map
        /// </summary>
        public double Y { get; set; }

        /// <summary>
        /// The size of the square/circle to display
        /// </summary>
        public double Size { get; set; }

        /// <summary>
        /// The name of the location
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The rewards for if this is not an actual location
        /// </summary>
        #nullable enable
        private BossInfo? Boss { get; set; }
        private DungeonInfo? Dungeon { get; set; }
        #nullable disable

        /// <summary>
        /// The number of available/accessible locations here that have not been
        /// cleared
        /// </summary>
        public int ClearableLocationsCount { get; set; }

        /// <summary>
        /// The total number of all locations here that have been cleared
        /// </summary>
        public int UnclearedLocationsCount { get; set; }

        public int OutOfLogicLocationsCount { get; set; }

        public int ClearedLocationsCount { get; set; }

        public int RelevantLocationsCount { get; set; }

        /// <summary>
        /// Display the icon if there are available uncleared locations that
        /// match the number of total uncleared locations
        /// </summary>
        public Visibility IconVisibility { get; set; }

        /// <summary>
        /// Get the icon to display for the location
        /// </summary>
        public ImageSource IconImage
        {
            get
            {
                var image = "blank.png";

                if (Locations == null)
                {
                    var region = (IHasReward)Region;
                    if (Boss != null && !Boss.Defeated && region.CanComplete(Syncer.ProgressionForRegion(Region)))
                    {
                        image = "boss.png";
                        IconVisibility = Visibility.Visible;
                    }
                    else if (Dungeon != null && !Dungeon.Cleared && region.CanComplete(Syncer.ProgressionForRegion(Region)))
                    {
                        image = Dungeon.Reward.GetDescription().ToLowerInvariant() + ".png";
                        IconVisibility = Visibility.Visible;
                    }
                }
                else
                {
                    if (ClearableLocationsCount > 0 && ClearableLocationsCount == UnclearedLocationsCount)
                    {
                        image = "accessible.png";
                        IconVisibility = Visibility.Visible;
                    }
                    else if (RelevantLocationsCount > 0 && RelevantLocationsCount + ClearableLocationsCount == UnclearedLocationsCount)
                    {
                        image = "relevant.png";
                        IconVisibility = Visibility.Visible;
                    }
                    else if (RelevantLocationsCount > 0 && ClearableLocationsCount == 0)
                    {
                        image = "partial_relevance.png";
                        IconVisibility = Visibility.Visible;
                    }
                    else if (ClearableLocationsCount > 0 && ClearableLocationsCount < UnclearedLocationsCount)
                    {
                        image = "partial.png";
                        IconVisibility = Visibility.Visible;
                    }
                    else if (ClearableLocationsCount == 0 && OutOfLogicLocationsCount > 0)
                    {
                        image = "outoflogic.png";
                        IconVisibility = Visibility.Visible;
                    }
                    else
                    {
                        IconVisibility = Visibility.Collapsed;
                    }
                }

                return new BitmapImage(new Uri(System.IO.Path.Combine(
                    System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    "Sprites", "Maps", image)));
            }
        }


        /// <summary>
        /// Get the icon to display for the number of locations
        /// </summary>
        public ImageSource NumberImage
        {
            get
            {
                if (ClearableLocationsCount > 1)
                {
                    return new BitmapImage(new Uri(System.IO.Path.Combine(
                        System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                        "Sprites", "Marks", $"{Math.Min(9, ClearableLocationsCount)}.png")));

                }
                else 
                {
                    return new BitmapImage(new Uri(System.IO.Path.Combine(
                        System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                        "Sprites", "Maps", "blank.png")));
                }
            }
        }
        

        /// <summary>
        /// The visual style for the right click menu
        /// </summary>
        public Style ContextMenuStyle => s_contextMenuStyle;

        /// <summary>
        /// The region for this location on the map
        /// </summary>
        public Region Region { get; }

        /// <summary>
        /// Get the tag to use for the location. Use the region for dungeons
        ///  and the list of locations for all other places
        /// </summary>
        public object Tag
        {
            get
            {
                if (Boss != null)
                {
                    return Boss;
                }
                else if (Dungeon != null)
                {
                    return Dungeon;
                }
                else
                {
                    return Region.Name == Name ? Region : Locations.Where(x => Syncer.IsLocationClearable(x, true, Syncer.Tracker.World.Config.Keysanity)).ToList();
                }
            }
        }

        /// <summary>
        /// LocationSyncer to use for keeping locations in sync between the
        /// locations list and map
        /// </summary>
        protected TrackerLocationSyncer Syncer { get; }
    }
}
