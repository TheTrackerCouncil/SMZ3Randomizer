using System.Collections.Generic;
using Randomizer.Shared;

namespace Randomizer.SMZ3.Regions.Zelda.LightWorld.DeathMountain
{
    public class LightWorldDeathMountainWest : Z3Region
    {
        public LightWorldDeathMountainWest(World world, Config config) : base(world, config)
        {
            EtherTablet = new Location(this, 256 + 0, 0x308016, LocationType.Ether,
                name: "Ether Tablet",
                vanillaItem: ItemType.Ether,
                access: items => items.Book && items.MasterSword && (items.Mirror || (items.Hammer && items.Hookshot)));

            SpectacleRock = new Location(this, 256 + 1, 0x308140, LocationType.Regular,
                name: "Spectacle Rock",
                vanillaItem: ItemType.HeartPiece,
                access: items => items.Mirror);

            SpectacleRockCave = new Location(this, 256 + 2, 0x308002, LocationType.Regular,
                "Spectacle Rock Cave",
                ItemType.HeartPiece);

            OldManReward = new Location(this, 256 + 3, 0x1EE9FA, LocationType.Regular,
                name: "Old Man",
                vanillaItem: ItemType.Mirror,
                access: items => Logic.CanPassSwordOnlyDarkRooms(items));
        }

        public override string Name => "Light World Death Mountain West";

        public override string Area => "Death Mountain";

        public Location EtherTablet { get; }

        public Location SpectacleRock { get; }

        public Location SpectacleRockCave { get; }

        public Location OldManReward { get; }

        public override bool CanEnter(Progression items)
        {
            return items.Flute
                || (Logic.CanLiftLight(items) && Logic.CanPassSwordOnlyDarkRooms(items))
                || Logic.CanAccessDeathMountainPortal(items);
        }
    }
}
