using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Randomizer.Data.WorldData.Regions;
using Randomizer.Data.WorldData.Regions.Zelda;
using Randomizer.Data.WorldData;
using Randomizer.Shared;
using Randomizer.Data.Configuration.ConfigTypes;
using static Randomizer.Data.Configuration.ConfigTypes.TrackerMapLocation;
using Randomizer.SMZ3.Tracking.Services;

namespace Randomizer.App.ViewModels
{
    /// <summary>
    /// ViewModel for displaying a specific location on the map and its progress
    /// </summary>
    public class TrackerMapLocationViewModel
    {
        private static readonly Style? s_contextMenuStyle =  Application.Current.FindResource("DarkContextMenu") as Style;

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
            Region = WorldService.Region(mapLocation.RegionTypeName) ?? throw new InvalidOperationException($"Map Region {mapLocation.RegionTypeName} not found");
            Type = mapLocation.Type;

            // If no location was specified, it's a boss or dungeon
            if (Type == MapLocationType.Boss)
            {
                if (Region.CanEnter(Syncer.ItemService.GetProgression(Region), true))
                {
                    if (Region is IHasReward rewardRegion)
                    {
                        RewardRegion = rewardRegion;
                        Name = RewardRegion.Reward.Type.GetDescription();
                        Y -= 22;
                    }
                    else if (Region is IHasBoss bossRegion)
                    {
                        BossRegion = bossRegion;
                        Name = bossRegion.Boss.Metadata.ToString() ?? bossRegion.Boss.Name;
                    }
                }
            }
            // Else figure out the current status of all of the locations
            else if (Type == MapLocationType.Item)
            {
                Name = mapLocation.GetName(syncer.WorldService.World);

                Locations = Syncer.WorldService.AllLocations().Where(mapLocation.MatchesSMZ3Location).ToList();
                var progression = Syncer.ItemService.GetProgression(!(Region is HyruleCastle || Region.World.Config.KeysanityForRegion(Region)));
                var statuses = Locations.Select(x => x.GetStatus(progression));

                ClearableLocationsCount = statuses.Count(x => x == Shared.Enums.LocationStatus.Available);
                RelevantLocationsCount = statuses.Count(x => x == Shared.Enums.LocationStatus.Relevant);
                OutOfLogicLocationsCount = Syncer.ShowOutOfLogicLocations ? statuses.Count(x => x == Shared.Enums.LocationStatus.OutOfLogic) : 0;
                UnclearedLocationsCount = statuses.Count(x => x != Shared.Enums.LocationStatus.Cleared);
                ClearedLocationsCount = statuses.Count() - UnclearedLocationsCount;

            }
            else if (Type == MapLocationType.SMDoor)
            {
                Item = Syncer.Tracker.ItemService.FirstOrDefault(mapLocation.Name);
                Name = "Need " + (Item?.Name ?? "Keycard");
            }

        }

        private IWorldService WorldService => Syncer.WorldService;

        /// <summary>
        /// Initializes a new instance of the <see cref="TrackerMap"/> class
        /// with data to display a specific map location in the sub menu
        /// </summary>
        /// <param name="location"></param>
        /// <param name="syncer"></param>
        public TrackerMapLocationViewModel(Location location, TrackerLocationSyncer syncer)
        {
            Locations = new List<Location>() { location };
            Region = location.Region;
            Syncer = syncer;
            Name = $"Clear {location.Metadata.Name[0]}";
        }

        /// <summary>
        /// The list of SMZ3 randomizer locations to look at to determine
        /// progress
        /// </summary>
        public List<Location> Locations { get; set; } = new();


        /// <summary>
        /// The list of locations underneath this one for the right click menu
        /// </summary>
        public List<TrackerMapLocationViewModel> SubLocationModels
            => Locations.Where(x => Syncer.WorldService.IsAvailable(x))
                .Select(x => new TrackerMapLocationViewModel(x, Syncer))
                .ToList();

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
        public string Name { get; set; } = "";

        /// <summary>
        /// The rewards for if this is not an actual location
        /// </summary>
        private IHasBoss? BossRegion { get; set; }
        private IHasReward? RewardRegion { get; set; }
        private Item? Item { get; set; }

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

        private MapLocationType Type { get; set; }

        /// <summary>
        /// Get the icon to display for the location
        /// </summary>
        public ImageSource IconImage
        {
            get
            {
                var image = "blank.png";

                if (Type == MapLocationType.Boss)
                {
                    var progression = Syncer.ItemService.GetProgression(Region);
                    var actualProgression = Syncer.ItemService.GetProgression(false);
                    if (BossRegion != null && BossRegion.Boss.State.Defeated != true && BossRegion.CanBeatBoss(progression))
                    {
                        image = "boss.png";
                    }
                    else if (RewardRegion != null && RewardRegion.Reward.State.Cleared != true)
                    {
                        var regionLocations = (IHasLocations)Region;

                        // If the player can complete the region with the current actual progression
                        // or if they can access all locations in the dungeon (unless this is Castle Tower
                        // in Keysanity because it doesn't have a location for Aga himself)
                        if (RewardRegion.CanComplete(actualProgression)
                            || (regionLocations.Locations.All(x => x.IsAvailable(progression, true))
                                && !(Region.Config.ZeldaKeysanity && RewardRegion is CastleTower)))
                        {
                            var dungeon = (IDungeon)Region;
                            image = dungeon.MarkedReward.GetDescription().ToLowerInvariant() + ".png";
                        }
                    }
                }
                else if (Type == MapLocationType.Item)
                {
                    if (ClearableLocationsCount > 0 && ClearableLocationsCount == UnclearedLocationsCount)
                    {
                        image = "accessible.png";
                    }
                    else if (RelevantLocationsCount > 0 && RelevantLocationsCount + ClearableLocationsCount == UnclearedLocationsCount)
                    {
                        image = "relevant.png";
                    }
                    else if (RelevantLocationsCount > 0 && ClearableLocationsCount == 0)
                    {
                        image = "partial_relevance.png";
                    }
                    else if (ClearableLocationsCount > 0 && ClearableLocationsCount < UnclearedLocationsCount)
                    {
                        image = "partial.png";
                    }
                    else if (ClearableLocationsCount == 0 && OutOfLogicLocationsCount > 0)
                    {
                        image = "outoflogic.png";
                    }
                }
                else if (Type == MapLocationType.SMDoor)
                {
                    if (Item is { State.TrackingState: 0 })
                    {
                        image = DoorImage;
                    }
                }

                IconVisibility = image == "blank.png" ? Visibility.Collapsed : Visibility.Visible;

                return new BitmapImage(new Uri(System.IO.Path.Combine(
                    System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
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
                        System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
                        "Sprites", "Marks", $"{Math.Min(9, ClearableLocationsCount)}.png")));

                }
                else
                {
                    return new BitmapImage(new Uri(System.IO.Path.Combine(
                        System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
                        "Sprites", "Maps", "blank.png")));
                }
            }
        }


        /// <summary>
        /// The visual style for the right click menu
        /// </summary>
        public Style? ContextMenuStyle => s_contextMenuStyle;

        /// <summary>
        /// The region for this location on the map
        /// </summary>
        public Region Region { get; }

        /// <summary>
        /// Get the tag to use for the location. Use the region for dungeons
        /// and the list of locations for all other places
        /// </summary>
        public object? Tag
        {
            get
            {
                if (Type == MapLocationType.Boss)
                {
                    return BossRegion == null ? RewardRegion : BossRegion;
                }
                else if (Type == MapLocationType.SMDoor)
                {
                    return Item;
                }
                else if (Type == MapLocationType.Item)
                {
                    return Region.Name == Name ? Region : Locations.Where(x => Syncer.WorldService.IsAvailable(x)).ToList();
                }
                return null;
            }
        }

        private string DoorImage => Item?.Type switch
        {
            ItemType.CardCrateriaL1 => "door1.png",
            ItemType.CardCrateriaL2 => "door2.png",
            ItemType.CardCrateriaBoss => "doorb.png",
            ItemType.CardBrinstarL1 => "door1.png",
            ItemType.CardBrinstarL2 => "door2.png",
            ItemType.CardBrinstarBoss => "doorb.png",
            ItemType.CardWreckedShipL1 => "door1.png",
            ItemType.CardWreckedShipBoss => "doorb.png",
            ItemType.CardMaridiaL1 => "door1.png",
            ItemType.CardMaridiaL2 => "door2.png",
            ItemType.CardMaridiaBoss => "doorb.png",
            ItemType.CardNorfairL1 => "door1.png",
            ItemType.CardNorfairL2 => "door2.png",
            ItemType.CardNorfairBoss => "doorb.png",
            ItemType.CardLowerNorfairL1 => "door1.png",
            ItemType.CardLowerNorfairBoss => "doorb.png",
            _ => "blank.png"
        };

        /// <summary>
        /// LocationSyncer to use for keeping locations in sync between the
        /// locations list and map
        /// </summary>
        protected TrackerLocationSyncer Syncer { get; }
    }
}
