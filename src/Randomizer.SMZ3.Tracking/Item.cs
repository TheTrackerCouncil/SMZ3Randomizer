using Randomizer.Shared;

namespace Randomizer.SMZ3.Tracking
{
    internal class Item
    {
        private ItemType _value;
        private World _world;

        public Item(ItemType value, World world)
        {
            _value = value;
            _world = world;
        }
    }
}
