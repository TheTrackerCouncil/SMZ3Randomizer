using System.Collections.Generic;
using Randomizer.Shared;
using static Randomizer.SMZ3.SMLogic;

namespace Randomizer.SMZ3.Regions.SuperMetroid.Norfair
{

    public class UpperNorfairWest : SMRegion
    {
        public UpperNorfairWest(World world, Config config) : base(world, config)
        {
            LavaRoom = new(this, 49, 0x8F8AE4, LocationType.Hidden,
                name: "Missile (lava room)",
                alsoKnownAs: new[] { "Lava Room - Submerged in wall", "Cathedral" },
                vanillaItem: ItemType.Missile,
                access: Logic switch
                {
                    Normal => items => items.Varia && (
                            (items.CanOpenRedDoors() && (items.CanFly() || items.HiJump || items.SpeedBooster)) ||
                            (World.UpperNorfairEast.CanEnter(items) && items.CardNorfairL2)
                        ) && items.Morph,
                    _ => items => items.CanHellRun() && (
                            items.CanOpenRedDoors() && (
                                items.CanFly() || items.HiJump || items.SpeedBooster ||
                                items.CanSpringBallJump() || (items.Varia && items.Ice)
                            ) ||
                            (World.UpperNorfairEast.CanEnter(items) && items.CardNorfairL2)
                        ) && items.Morph,
                });
            IceBeamRoom = new(this, 50, 0x8F8B24, LocationType.Chozo,
                name: "Ice Beam",
                alsoKnownAs: "Ice Beam Room",
                vanillaItem: ItemType.Ice,
                access: Logic switch
                {
                    Normal => items => (Config.Keysanity ? items.CardNorfairL1 : items.Super) && items.CanPassBombPassages() && items.Varia && items.SpeedBooster,
                    _ => items => (Config.Keysanity ? items.CardNorfairL1 : items.Super) && items.Morph && (items.Varia || items.HasEnergyReserves(3))
                });
            CrumbleShaft = new(this, 51, 0x8F8B46, LocationType.Hidden,
                name: "Missile (below Ice Beam)",
                alsoKnownAs: "Crumble Shaft",
                vanillaItem: ItemType.Missile,
                access: Logic switch
                {
                    Normal => items => (Config.Keysanity ? items.CardNorfairL1 : items.Super) && items.CanUsePowerBombs() && items.Varia && items.SpeedBooster,
                    _ => items =>
                        ((Config.Keysanity ? items.CardNorfairL1 : items.Super) && items.CanUsePowerBombs() && (items.Varia || items.HasEnergyReserves(3))) ||
                        ((items.Missile || items.Super || items.Wave) /* Blue Gate */ && items.Varia && items.SpeedBooster &&
                        /* Access to Croc's room to get spark */
                        (Config.Keysanity ? items.CardNorfairBoss : items.Super) && items.CardNorfairL1)
                });
            HiJumpBootsRoom = new(this, 53, 0x8F8BAC, LocationType.Chozo,
                name: "Hi-Jump Boots",
                alsoKnownAs: "Hi-Jump Boots Room",
                vanillaItem: ItemType.HiJump,
                access: Logic switch
                {
                    _ => items => items.CanOpenRedDoors() && items.CanPassBombPassages()
                });
            HiJumpLobbyBack = new(this, 55, 0x8F8BE6, LocationType.Visible,
                name: "Missile (Hi-Jump Boots)",
                alsoKnownAs: "Hi-Jump Lobby - Back",
                vanillaItem: ItemType.Missile,
                access: Logic switch
                {
                    _ => items => items.CanOpenRedDoors() && items.Morph
                });
            HiJumpLobbyEntrance = new(this, 56, 0x8F8BEC, LocationType.Visible,
                name: "Energy Tank (Hi-Jump Boots)",
                alsoKnownAs: "Hi-Jump Lobby - Entrance",
                vanillaItem: ItemType.ETank,
                access: Logic switch
                {
                    _ => items => items.CanOpenRedDoors()
                });
        }
        public override string Name => "Upper Norfair, West";
        public override string Area => "Upper Norfair";

        public Location LavaRoom { get; }

        public Location IceBeamRoom { get; }

        public Location CrumbleShaft { get; }

        public Location HiJumpBootsRoom { get; }

        public Location HiJumpLobbyBack { get; }

        public Location HiJumpLobbyEntrance { get; }

        public override bool CanEnter(Progression items)
        {
            return (items.CanDestroyBombWalls() || items.SpeedBooster) && items.Super && items.Morph ||
                items.CanAccessNorfairUpperPortal();
        }

    }

}
