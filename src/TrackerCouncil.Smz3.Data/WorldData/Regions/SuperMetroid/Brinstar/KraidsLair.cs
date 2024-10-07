using System.Collections.Generic;
using TrackerCouncil.Smz3.Shared;
using TrackerCouncil.Smz3.Data.Configuration.ConfigTypes;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Data.Services;
using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.Shared.Models;

namespace TrackerCouncil.Smz3.Data.WorldData.Regions.SuperMetroid.Brinstar;

public class KraidsLair : SMRegion, IHasBoss, IHasReward
{
    public KraidsLair(World world, Config config, IMetadataService? metadata, TrackerState? trackerState) : base(world, config, metadata, trackerState)
    {
        WarehouseEnergyTank = new WarehouseEnergyTankRoom(this, metadata, trackerState);
        WarehouseKihunter = new WarehouseKihunterRoom(this, metadata, trackerState);
        VariaSuit = new VariaSuitRoom(this, metadata, trackerState);
        MemoryRegionId = 1;
        Metadata = metadata?.Region(GetType()) ?? new RegionInfo("Kraid's Lair");
        MapName = "Brinstar";

        ((IHasReward)this).ApplyState(trackerState);
        ((IHasBoss)this).ApplyState(trackerState);
    }

    public override string Name => "Kraid's Lair";

    public override string Area => "Brinstar";

    public override List<string> AlsoKnownAs { get; } = ["Warehouse"];

    public Boss Boss { get; set; } = null!;

    public BossType DefaultBossType => BossType.Kraid;

    public LocationId? BossLocationId => LocationId.KraidsLairVariaSuit;

    public Reward Reward { get; set; } = null!;

    public RewardType DefaultRewardType => RewardType.MetroidBoss;

    public TrackerRewardState RewardState { get; set; } = null!;

    public bool IsShuffledReward => false;

    public WarehouseEnergyTankRoom WarehouseEnergyTank { get; }

    public WarehouseKihunterRoom WarehouseKihunter { get; }

    public VariaSuitRoom VariaSuit { get; }

    public override bool CanEnter(Progression items, bool requireRewards)
    {
        return (Logic.CanDestroyBombWalls(items) || items.SpeedBooster || Logic.CanAccessNorfairUpperPortal(items))
               && items.Super && Logic.CanPassBombPassages(items)
               && (items.HiJump || Logic.CanWallJump(WallJumpDifficulty.Medium) || Logic.CanFly(items));
    }

    public bool CanBeatBoss(Progression items)
    {
        return CanEnter(items, true) && items.CardBrinstarBoss;
    }

    public bool CanRetrieveReward(Progression items) => CanBeatBoss(items);

    public bool CanSeeReward(Progression items) => true;

    public class WarehouseEnergyTankRoom : Room
    {
        public WarehouseEnergyTankRoom(KraidsLair region, IMetadataService? metadata, TrackerState? trackerState)
            : base(region, "Warehouse Energy Tank Room", metadata)
        {
            Locations = new List<Location>
            {
                new Location(this, LocationId.KraidsLairEnergyTank, 0x8F899C, LocationType.Hidden,
                    name: "Energy Tank, Kraid",
                    vanillaItem: ItemType.ETank,
                    access: items => items.Kraid,
                    relevanceRequirement: items => region.CanBeatBoss(items),
                    memoryAddress: 0x5,
                    memoryFlag: 0x8,
                    metadata: metadata,
                    trackerState: trackerState)
            };
        }
    }

    public class WarehouseKihunterRoom : Room
    {
        public WarehouseKihunterRoom(KraidsLair region, IMetadataService? metadata, TrackerState? trackerState)
            : base(region, "Warehouse Kihunter Room", metadata)
        {
            Locations = new List<Location>
            {
                new Location(this, LocationId.KraidsLairKihunter, 0x8F89EC, LocationType.Hidden,
                    name: "Missile (Kraid)",
                    vanillaItem: ItemType.Missile,
                    access: items => Logic.CanUsePowerBombs(items),
                    memoryAddress: 0x5,
                    memoryFlag: 0x10,
                    metadata: metadata,
                    trackerState: trackerState)
            };
        }
    }

    public class VariaSuitRoom : Room
    {
        public VariaSuitRoom(KraidsLair region, IMetadataService? metadata, TrackerState? trackerState)
            : base(region, "Varia Suit Room", metadata)
        {
            Locations = new List<Location>
            {
                new Location(this, LocationId.KraidsLairVariaSuit, 0x8F8ACA, LocationType.Chozo,
                    name: "Varia Suit",
                    vanillaItem: ItemType.Varia,
                    access: items => items.Kraid,
                    relevanceRequirement: items => region.CanBeatBoss(items),
                    memoryAddress: 0x6,
                    memoryFlag: 0x1,
                    metadata: metadata,
                    trackerState: trackerState)
            };
        }
    }
}
