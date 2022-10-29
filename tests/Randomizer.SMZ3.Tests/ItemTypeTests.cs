using System;
using System.Linq;
using Randomizer.Shared;
using Randomizer.Shared.Enums;
using Xunit;

namespace Randomizer.SMZ3.Tests
{
    public class ItemTypeTests
    {
        [Theory]
        [InlineData("Map", ItemCategory.Map)]
        [InlineData("Compass", ItemCategory.Compass)]
        [InlineData("BigKey", ItemCategory.BigKey)]
        [InlineData("Key", ItemCategory.SmallKey)]
        [InlineData("Card", ItemCategory.Keycard)]
        public void DungeonSpecificItemsHaveCorrectCategories(string prefix, ItemCategory expectedCategory)
        {
            foreach (var itemType in Enum.GetValues<ItemType>().Where(x => x.ToString().StartsWith(prefix)))
            {
                Assert.True(itemType.IsInCategory(expectedCategory));
            }
        }

        [Fact]
        public void DungeonSpecificItemsAreDungeonItems()
        {
            var itemTypes = Enum.GetValues<ItemType>()
                .Where(x => x.ToString().StartsWith("Map")
                            || x.ToString().StartsWith("Compass")
                            || x.ToString().StartsWith("BigKey")
                            || x.ToString().StartsWith("Key"));
            foreach (var itemType in itemTypes)
            {
                var result = itemType.IsInAnyCategory(ItemCategory.BigKey,
                    ItemCategory.Compass, ItemCategory.SmallKey, ItemCategory.Map);
                Assert.True(result, $"{itemType} should be considered a dungeon item");
            }
        }

        [Theory]
        [InlineData(ItemType.SilverArrows)]
        [InlineData(ItemType.ThreeBombs)]
        public void ZeldaItemsAreNotMetroidItems(ItemType itemType)
        {
            var result = itemType.IsInCategory(ItemCategory.Metroid);
            Assert.False(result);
        }
    }
}
