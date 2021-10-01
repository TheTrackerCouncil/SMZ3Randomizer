using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

namespace Randomizer.SMZ3.Tests
{
    [TestFixture]
    public class ItemTypeTests
    {
        [TestCase("Map", ItemCategory.Map)]
        [TestCase("Compass", ItemCategory.Compass)]
        [TestCase("BigKey", ItemCategory.BigKey)]
        [TestCase("Key", ItemCategory.SmallKey)]
        [TestCase("Card", ItemCategory.Keycard)]
        public void DungeonSpecificItemsHaveCorrectCategories(string prefix, ItemCategory expectedCategory)
        {
            foreach (var itemType in Enum.GetValues<ItemType>().Where(x => x.ToString().StartsWith(prefix)))
            {
                Assert.IsTrue(itemType.IsInCategory(expectedCategory));
            }
        }

        [Test]
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
                Assert.IsTrue(result, $"{itemType} should be considered a dungeon item");
            }
        }

        [TestCase(ItemType.SilverArrows)]
        [TestCase(ItemType.ThreeBombs)]
        public void ZeldaItemsAreNotMetroidItems(ItemType itemType)
        {
            var result = itemType.IsInCategory(ItemCategory.Metroid);
            Assert.IsFalse(result);
        }
    }
}
