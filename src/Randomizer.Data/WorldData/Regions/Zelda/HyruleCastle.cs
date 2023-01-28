using System.Collections.Generic;
using System.Linq;
using Randomizer.Shared;
using Randomizer.Data.Options;
using Randomizer.Data.Configuration.ConfigTypes;
using Randomizer.Data.Services;
using Randomizer.Shared.Models;

namespace Randomizer.Data.WorldData.Regions.Zelda
{
    public class HyruleCastle : Z3Region, IDungeon
    {
        public const int SphereOne = -10;

        public HyruleCastle(World world, Config config, IMetadataService? metadata, TrackerState? trackerState) : base(world, config, metadata, trackerState)
        {
            RegionItems = new[] { ItemType.KeyHC, ItemType.MapHC };

            Sanctuary = new Location(this, 256 + 91, 0x1EA79, LocationType.Regular,
                name: "Sanctuary",
                vanillaItem: ItemType.HeartContainer,
                memoryAddress: 0x12,
                memoryFlag: 0x4,
                metadata: metadata,
                trackerState: trackerState)
                .Weighted(SphereOne);

            MapChest = new Location(this, 256 + 96, 0x1EB0C, LocationType.Regular,
                name: "Map Chest",
                vanillaItem: ItemType.MapHC,
                memoryAddress: 0x72,
                memoryFlag: 0x4,
                metadata: metadata,
                trackerState: trackerState)
                .Weighted(SphereOne);

            BoomerangChest = new Location(this, 256 + 97, 0x1E974, LocationType.Regular,
                name: "Boomerang Chest",
                vanillaItem: ItemType.BlueBoomerang,
                access: items => items.KeyHC,
                memoryAddress: 0x71,
                memoryFlag: 0x4,
                metadata: metadata,
                trackerState: trackerState)
                .Weighted(SphereOne);

            ZeldasCell = new Location(this, 256 + 98, 0x1EB09, LocationType.Regular,
                name: "Zelda's Cell",
                vanillaItem: ItemType.FiveRupees,
                access: items => items.KeyHC,
                memoryAddress: 0x80,
                memoryFlag: 0x4,
                metadata: metadata,
                trackerState: trackerState)
                .Weighted(SphereOne);

            LinksUncle = new Location(this, 256 + 99, 0x5DF45, LocationType.NotInDungeon,
                name: "Link's Uncle",
                vanillaItem: ItemType.ProgressiveSword,
                memoryAddress: 0x146,
                memoryFlag: 0x1,
                memoryType: LocationMemoryType.ZeldaMisc,
                metadata: metadata,
                trackerState: trackerState)
                .Allow((item, items) => Config.ZeldaKeysanity || !item.IsDungeonItem)
                .Weighted(SphereOne);

            SecretPassage = new Location(this, 256 + 100, 0x1E971, LocationType.NotInDungeon,
                name: "Secret Passage",
                vanillaItem: ItemType.FiveRupees,
                memoryAddress: 0x55,
                memoryFlag: 0x4,
                metadata: metadata,
                trackerState: trackerState)
                .Allow((item, items) => Config.ZeldaKeysanity || !item.IsDungeonItem)
                .Weighted(SphereOne);

            BackOfEscape = new BackOfEscapeRoom(this, metadata, trackerState);

            StartingRooms = new List<int> { 96, 97, 98 };
            Metadata = metadata?.Region(GetType()) ?? new RegionInfo("Hyrule Castle");
            DungeonMetadata = metadata?.Dungeon(GetType()) ?? new DungeonInfo("Hyrule Castle", "HC", "Ball and Chain Soldier");
            DungeonState = trackerState?.DungeonStates.First(x => x.WorldId == world.Id && x.Name == GetType().Name) ?? new TrackerDungeonState();
        }

        public override string Name => "Hyrule Castle";

        public int SongIndex { get; init; } = 0;

        public DungeonInfo DungeonMetadata { get; set; }

        public TrackerDungeonState DungeonState { get; set; }

        public Location Sanctuary { get; }

        public Location MapChest { get; }

        public Location BoomerangChest { get; }

        public Location ZeldasCell { get; }

        public Location LinksUncle { get; }

        public Location SecretPassage { get; }

        public BackOfEscapeRoom BackOfEscape { get; }

        public Region ParentRegion => World.LightWorldNorthEast;

        public class BackOfEscapeRoom : Room
        {
            public BackOfEscapeRoom(Region region, IMetadataService? metadata, TrackerState? trackerState)
                : base(region, "Sewers", metadata, "Back of Escape")
            {
                SecretRoomLeft = new Location(this, 256 + 92, 0x1EB5D, LocationType.Regular,
                    name: "Secret Room - Left",
                    vanillaItem: ItemType.ThreeBombs,
                    access: items => Logic.CanLiftLight(items) || (Logic.CanPassFireRodDarkRooms(items) && items.KeyHC),
                    memoryAddress: 0x11,
                    memoryFlag: 0x4,
                    metadata: metadata,
                    trackerState: trackerState);

                SecretRoomMiddle = new Location(this, 256 + 93, 0x1EB60, LocationType.Regular,
                    name: "Secret Room - Middle",
                    vanillaItem: ItemType.ThreeHundredRupees,
                    access: items => Logic.CanLiftLight(items) || (Logic.CanPassFireRodDarkRooms(items) && items.KeyHC),
                    memoryAddress: 0x11,
                    memoryFlag: 0x5,
                    metadata: metadata,
                    trackerState: trackerState);

                SecretRoomRight = new Location(this, 256 + 94, 0x1EB63, LocationType.Regular,
                    name: "Secret Room - Right",
                    vanillaItem: ItemType.TenArrows,
                    access: items => Logic.CanLiftLight(items) || (Logic.CanPassFireRodDarkRooms(items) && items.KeyHC),
                    memoryAddress: 0x11,
                    memoryFlag: 0x6,
                    metadata: metadata,
                    trackerState: trackerState);

                DarkCross = new Location(this, 256 + 95, 0x1E96E, LocationType.Regular,
                    name: "Dark Cross",
                    vanillaItem: ItemType.KeyHC,
                    access: items => Logic.CanPassFireRodDarkRooms(items),
                    memoryAddress: 0x32,
                    memoryFlag: 0x4,
                    metadata: metadata,
                    trackerState: trackerState);
            }

            public Location SecretRoomLeft { get; }

            public Location SecretRoomMiddle { get; }

            public Location SecretRoomRight { get; }

            public Location DarkCross { get; }
        }
    }
}
