using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FluentAssertions;

using Randomizer.Shared;

using Xunit;

namespace Randomizer.SMZ3.Tests.LogicTests
{
    public class LocationLogicTests
    {
        public LocationLogicTests()
        {
            Config = new Config();
            World = new World(Config, "", 0, "");
        }

        protected Config Config { get; }

        protected World World { get; }

        [Fact]
        public void LocationWithoutLogicNeverHasMissingItems()
        {
            var emptyProgression = new Progression();
            var missingItems = Logic.GetMissingRequiredItems(World.HyruleCastle.LinksUncle, emptyProgression);
            missingItems.Should().BeEmpty();
        }

        [Fact]
        public void LocationWithSatisfiedLogicHasNoMissingItems()
        {
            var progression = new Progression(new[] { new Item(ItemType.Boots) });
            var missingItems = Logic.GetMissingRequiredItems(World.LightWorldSouth.Library, progression);
            missingItems.Should().BeEmpty();
        }

        [Fact]
        public void LocationWithSimpleLogicOnlyHasOneSetOfItems()
        {
            var emptyProgression = new Progression();
            var missingItems = Logic.GetMissingRequiredItems(World.LightWorldSouth.Library, emptyProgression);
            missingItems.Should().HaveCount(1);
        }

        [Fact]
        public void LocationWithSimpleLogicReturnsSingleMissingItem()
        {
            var emptyProgression = new Progression();
            var missingItems = Logic.GetMissingRequiredItems(World.LightWorldSouth.Library, emptyProgression);
            missingItems.Should().ContainEquivalentOf(new[] { ItemType.Boots });
        }

        [Fact]
        public void LocationWithSimpleLogicReturnsBothMissingItems()
        {
            var emptyProgression = new Progression(Item.CreateKeycards(null));
            var missingItems = Logic.GetMissingRequiredItems(World.GreenBrinstar.ETank, emptyProgression);
            missingItems.Should().ContainEquivalentOf(new[] { ItemType.Morph, ItemType.PowerBomb });
        }
    }
}
