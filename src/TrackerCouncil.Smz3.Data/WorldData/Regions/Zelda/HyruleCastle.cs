using System;
using System.Collections.Generic;
using System.Linq;
using TrackerCouncil.Smz3.Shared;
using TrackerCouncil.Smz3.Data.Configuration.ConfigTypes;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Data.Services;
using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.Shared.Models;

namespace TrackerCouncil.Smz3.Data.WorldData.Regions.Zelda;

public class HyruleCastle : Z3Region, IHasTreasure, IHasBoss
{
    public const int SphereOne = -10;

    public HyruleCastle(World world, Config config, IMetadataService? metadata, TrackerState? trackerState) : base(world, config, metadata, trackerState)
    {
        RegionItems = [ItemType.KeyHC, ItemType.MapHC];

        Sanctuary = new Location(this, LocationId.Sanctuary, 0x1EA79, LocationType.Regular,
                name: "Sanctuary",
                vanillaItem: ItemType.HeartContainer,
                memoryAddress: 0x12,
                memoryFlag: 0x4,
                metadata: metadata,
                trackerState: trackerState)
            .Weighted(SphereOne);

        MapChest = new Location(this, LocationId.HyruleCastleMapChest, 0x1EB0C, LocationType.Regular,
                name: "Map Chest",
                vanillaItem: ItemType.MapHC,
                memoryAddress: 0x72,
                memoryFlag: 0x4,
                metadata: metadata,
                trackerState: trackerState)
            .Weighted(SphereOne);

        BoomerangChest = new Location(this, LocationId.HyruleCastleBoomerangChest, 0x1E974, LocationType.Regular,
                name: "Boomerang Chest",
                vanillaItem: ItemType.BlueBoomerang,
                access: items => items.KeyHC,
                memoryAddress: 0x71,
                memoryFlag: 0x4,
                metadata: metadata,
                trackerState: trackerState)
            .Weighted(SphereOne);

        ZeldasCell = new Location(this, LocationId.HyruleCastleZeldasCell, 0x1EB09, LocationType.Regular,
                name: "Zelda's Cell",
                vanillaItem: ItemType.FiveRupees,
                access: items => items.KeyHC,
                memoryAddress: 0x80,
                memoryFlag: 0x4,
                metadata: metadata,
                trackerState: trackerState)
            .Weighted(SphereOne);

        LinksUncle = new Location(this, LocationId.LinksUncle, 0x5DF45, LocationType.NotInDungeon,
                name: "Link's Uncle",
                vanillaItem: ItemType.ProgressiveSword,
                memoryAddress: 0x146,
                memoryFlag: 0x1,
                memoryType: LocationMemoryType.ZeldaMisc,
                metadata: metadata,
                trackerState: trackerState)
            .Allow((item, items) => Config.ZeldaKeysanity || !item.IsDungeonItem)
            .Weighted(SphereOne);

        SecretPassage = new Location(this, LocationId.SecretPassage, 0x1E971, LocationType.NotInDungeon,
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
        MapName = "Light World";

        ((IHasTreasure)this).ApplyState(trackerState);
        ((IHasBoss)this).ApplyState(trackerState);
    }

    public override string Name => "Hyrule Castle";

    public BossType DefaultBossType => BossType.CastleGuard;

    public LocationId? BossLocationId => null;

    public bool UnifiedBossAndItemLocation => true;

    public TrackerTreasureState TreasureState { get; set; } = null!;

    public event EventHandler? UpdatedTreasure;

    public void OnUpdatedTreasure() => UpdatedTreasure?.Invoke(this, EventArgs.Empty);

    public Boss Boss { get; set; } = null!;

    public bool CanBeatBoss(Progression items) => ZeldasCell.IsAvailable(items);

    public Location Sanctuary { get; }

    public Location MapChest { get; }

    public Location BoomerangChest { get; }

    public Location ZeldasCell { get; }

    public Location LinksUncle { get; }

    public Location SecretPassage { get; }

    public BackOfEscapeRoom BackOfEscape { get; }

    public class BackOfEscapeRoom : Room
    {
        public BackOfEscapeRoom(Region region, IMetadataService? metadata, TrackerState? trackerState)
            : base(region, "Sewers", metadata, "Back of Escape")
        {
            Locations = new List<Location>
            {
                new Location(this, LocationId.SewersSecretRoomLeft, 0x1EB5D, LocationType.Regular,
                    name: "Secret Room - Left",
                    vanillaItem: ItemType.ThreeBombs,
                    access: items => Logic.CanLiftLight(items) || (Logic.CanPassFireRodDarkRooms(items) && items.KeyHC),
                    memoryAddress: 0x11,
                    memoryFlag: 0x4,
                    metadata: metadata,
                    trackerState: trackerState),
                new Location(this, LocationId.SewersSecretRoomMiddle, 0x1EB60, LocationType.Regular,
                    name: "Secret Room - Middle",
                    vanillaItem: ItemType.ThreeHundredRupees,
                    access: items => Logic.CanLiftLight(items) || (Logic.CanPassFireRodDarkRooms(items) && items.KeyHC),
                    memoryAddress: 0x11,
                    memoryFlag: 0x5,
                    metadata: metadata,
                    trackerState: trackerState),
                new Location(this, LocationId.SewersSecretRoomRight, 0x1EB63, LocationType.Regular,
                    name: "Secret Room - Right",
                    vanillaItem: ItemType.TenArrows,
                    access: items => Logic.CanLiftLight(items) || (Logic.CanPassFireRodDarkRooms(items) && items.KeyHC),
                    memoryAddress: 0x11,
                    memoryFlag: 0x6,
                    metadata: metadata,
                    trackerState: trackerState),
                new Location(this, LocationId.SewersDarkCross, 0x1E96E, LocationType.Regular,
                    name: "Dark Cross",
                    vanillaItem: ItemType.KeyHC,
                    access: items => Logic.CanPassFireRodDarkRooms(items),
                    memoryAddress: 0x32,
                    memoryFlag: 0x4,
                    metadata: metadata,
                    trackerState: trackerState)
            };
        }
    }
}
