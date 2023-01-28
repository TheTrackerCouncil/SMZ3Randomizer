using Randomizer.Data.Configuration.ConfigTypes;
using Randomizer.Shared;
using Randomizer.Data.Options;
using Randomizer.Data.Services;
using Randomizer.Shared.Models;

namespace Randomizer.Data.WorldData.Regions.SuperMetroid.Norfair
{

    public class UpperNorfairWest : SMRegion
    {
        public UpperNorfairWest(World world, Config config, IMetadataService? metadata, TrackerState? trackerState) : base(world, config, metadata, trackerState)
        {
            LavaRoom = new Location(this, 49, 0x8F8AE4, LocationType.Hidden,
                name: "Missile (lava room)",
                alsoKnownAs: new[] { "Lava Room - Submerged in wall", "Cathedral" },
                vanillaItem: ItemType.Missile,
                access: items => items.Varia && (
                            (Logic.CanOpenRedDoors(items) && (Logic.CanFly(items) || items.HiJump || items.SpeedBooster)) ||
                            (World.UpperNorfairEast.CanEnter(items, true) && items.CardNorfairL2)
                        ) && items.Morph,
                memoryAddress: 0x6,
                memoryFlag: 0x2,
                metadata: metadata,
                trackerState: trackerState);
            IceBeamRoom = new Location(this, 50, 0x8F8B24, LocationType.Chozo,
                name: "Ice Beam",
                alsoKnownAs: new[] { "Ice Beam Room" },
                vanillaItem: ItemType.Ice,
                access: items => (Config.MetroidKeysanity ? items.CardNorfairL1 : items.Super)
                                 && Logic.CanPassBombPassages(items) && items.Varia && Logic.CanMoveAtHighSpeeds(items),
                memoryAddress: 0x6,
                memoryFlag: 0x4,
                metadata: metadata,
                trackerState: trackerState);
            CrumbleShaft = new Location(this, 51, 0x8F8B46, LocationType.Hidden,
                name: "Missile (below Ice Beam)",
                alsoKnownAs: new[] { "Crumble Shaft" },
                vanillaItem: ItemType.Missile,
                access: items => (Config.MetroidKeysanity ? items.CardNorfairL1 : items.Super)
                                 && Logic.CanUsePowerBombs(items) && items.Varia && Logic.CanMoveAtHighSpeeds(items)
                                 && (Logic.CanWallJump(WallJumpDifficulty.Easy) || Logic.CanFly(items)),
                memoryAddress: 0x6,
                memoryFlag: 0x8,
                metadata: metadata,
                trackerState: trackerState);
            HiJumpBootsRoom = new Location(this, 53, 0x8F8BAC, LocationType.Chozo,
                name: "Hi-Jump Boots",
                alsoKnownAs: new[] { "Hi-Jump Boots Room" },
                vanillaItem: ItemType.HiJump,
                access: items => Logic.CanOpenRedDoors(items) && Logic.CanPassBombPassages(items),
                memoryAddress: 0x6,
                memoryFlag: 0x20,
                metadata: metadata,
                trackerState: trackerState);
            HiJumpLobbyBack = new Location(this, 55, 0x8F8BE6, LocationType.Visible,
                name: "Missile (Hi-Jump Boots)",
                alsoKnownAs: new[] { "Hi-Jump Lobby - Back" },
                vanillaItem: ItemType.Missile,
                access: items => Logic.CanOpenRedDoors(items) && items.Morph,
                memoryAddress: 0x6,
                memoryFlag: 0x80,
                metadata: metadata,
                trackerState: trackerState);
            HiJumpLobbyEntrance = new Location(this, 56, 0x8F8BEC, LocationType.Visible,
                name: "Energy Tank (Hi-Jump Boots)",
                alsoKnownAs: new[] { "Hi-Jump Lobby - Entrance" },
                vanillaItem: ItemType.ETank,
                access: items => Logic.CanOpenRedDoors(items),
                memoryAddress: 0x7,
                memoryFlag: 0x1,
                metadata: metadata,
                trackerState: trackerState);
            MemoryRegionId = 2;
            Metadata = metadata?.Region(GetType()) ?? new RegionInfo("Upper Norfair West");
        }
        public override string Name => "Upper Norfair, West";
        public override string Area => "Upper Norfair";

        public Location LavaRoom { get; }

        public Location IceBeamRoom { get; }

        public Location CrumbleShaft { get; }

        public Location HiJumpBootsRoom { get; }

        public Location HiJumpLobbyBack { get; }

        public Location HiJumpLobbyEntrance { get; }

        public override bool CanEnter(Progression items, bool requireRewards)
        {
            return (Logic.CanDestroyBombWalls(items) || items.SpeedBooster) && items.Super && items.Morph ||
                Logic.CanAccessNorfairUpperPortal(items);
        }

    }

}
