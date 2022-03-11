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
                access: items => items.Varia && (
                            (Logic.CanOpenRedDoors(items) && (Logic.CanFly(items) || items.HiJump || items.SpeedBooster)) ||
                            (World.UpperNorfairEast.CanEnter(items) && items.CardNorfairL2)
                        ) && items.Morph);
            IceBeamRoom = new(this, 50, 0x8F8B24, LocationType.Chozo,
                name: "Ice Beam",
                alsoKnownAs: "Ice Beam Room",
                vanillaItem: ItemType.Ice,
                access: items => (Config.Keysanity ? items.CardNorfairL1 : items.Super) && Logic.CanPassBombPassages(items) && items.Varia && Logic.CanMoveAtHighSpeeds(items));
            CrumbleShaft = new(this, 51, 0x8F8B46, LocationType.Hidden,
                name: "Missile (below Ice Beam)",
                alsoKnownAs: "Crumble Shaft",
                vanillaItem: ItemType.Missile,
                access: items => (Config.Keysanity ? items.CardNorfairL1 : items.Super) && Logic.CanUsePowerBombs(items) && items.Varia && Logic.CanMoveAtHighSpeeds(items));
            HiJumpBootsRoom = new(this, 53, 0x8F8BAC, LocationType.Chozo,
                name: "Hi-Jump Boots",
                alsoKnownAs: "Hi-Jump Boots Room",
                vanillaItem: ItemType.HiJump,
                access: items => Logic.CanOpenRedDoors(items) && Logic.CanPassBombPassages(items));
            HiJumpLobbyBack = new(this, 55, 0x8F8BE6, LocationType.Visible,
                name: "Missile (Hi-Jump Boots)",
                alsoKnownAs: "Hi-Jump Lobby - Back",
                vanillaItem: ItemType.Missile,
                access: items => Logic.CanOpenRedDoors(items) && items.Morph);
            HiJumpLobbyEntrance = new(this, 56, 0x8F8BEC, LocationType.Visible,
                name: "Energy Tank (Hi-Jump Boots)",
                alsoKnownAs: "Hi-Jump Lobby - Entrance",
                vanillaItem: ItemType.ETank,
                access: items => Logic.CanOpenRedDoors(items));
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
            return (Logic.CanDestroyBombWalls(items) || items.SpeedBooster) && items.Super && items.Morph ||
                Logic.CanAccessNorfairUpperPortal(items);
        }

    }

}
