using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using FluentAssertions;

using Randomizer.Shared;

using Xunit;

namespace Randomizer.SMZ3.Tests.LogicTests
{
    public class LogicConfigTests
    {
        [Fact]
        public void ValidateConfigDetails()
        {
            Config config = new Config();
            var type = config.LogicConfig.GetType();

            foreach (var property in type.GetProperties())
            {
                var displayName = property.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName;
                displayName.Should().NotBeNullOrEmpty();

                var category = property.GetCustomAttribute<CategoryAttribute>().Category;
                category.Should().BeOneOf("Logic", "Tricks");
            }
        }

        [Fact]
        public void TestPreventScrewAttackSoftLock()
        {
            Config config = new Config();

            config.LogicConfig.PreventScrewAttackSoftLock = false;
            World tempWorld = new World(config, "", 0, "");
            var progression = new Progression(new[] { ItemType.ScrewAttack });
            var missingItems = Logic.GetMissingRequiredItems(tempWorld.WestCrateria.Terminator, progression);
            missingItems.Should().BeEmpty();

            config.LogicConfig.PreventScrewAttackSoftLock = true;
            tempWorld = new World(config, "", 0, "");
            progression = new Progression(new[] { ItemType.ScrewAttack });
            missingItems = Logic.GetMissingRequiredItems(tempWorld.WestCrateria.Terminator, progression);
            missingItems.Should().HaveCount(1);
            missingItems.Should().ContainEquivalentOf(new[] { ItemType.Morph });
        }

        [Fact]
        public void TestRequireTwoPowerBombs()
        {
            Config config = new Config();

            config.LogicConfig.RequireTwoPowerBombs = false;
            World tempWorld = new World(config, "", 0, "");
            var progression = new Progression(new[] { ItemType.Morph, ItemType.PowerBomb });
            var missingItems = Logic.GetMissingRequiredItems(tempWorld.WestCrateria.Terminator, progression);
            missingItems.Should().BeEmpty();

            config.LogicConfig.RequireTwoPowerBombs = true;
            tempWorld = new World(config, "", 0, "");
            progression = new Progression(new[] { ItemType.Morph, ItemType.PowerBomb });
            missingItems = Logic.GetMissingRequiredItems(tempWorld.WestCrateria.Terminator, progression);
            missingItems.Should().HaveCount(3);
            missingItems.Should().ContainEquivalentOf(new[] { ItemType.Bombs });
            missingItems.Should().ContainEquivalentOf(new[] { ItemType.ScrewAttack });
            missingItems.Should().ContainEquivalentOf(new[] { ItemType.PowerBomb });

            progression = new Progression(new[] { ItemType.Morph, ItemType.PowerBomb, ItemType.PowerBomb });
            missingItems = Logic.GetMissingRequiredItems(tempWorld.WestCrateria.Terminator, progression);
            missingItems.Should().BeEmpty();
        }

        [Fact]
        public void TestFireRodDarkRooms()
        {
            Config config = new Config();

            config.LogicConfig.FireRodDarkRooms = false;
            World tempWorld = new World(config, "", 0, "");
            var progression = new Progression();
            var missingItems = Logic.GetMissingRequiredItems(tempWorld.HyruleCastle.BackOfEscape.DarkCross, progression);
            missingItems.Should().HaveCount(1);
            missingItems.Should().ContainEquivalentOf(new[] { ItemType.Lamp });

            config.LogicConfig.FireRodDarkRooms = true;
            tempWorld = new World(config, "", 0, "");
            progression = new Progression();
            missingItems = Logic.GetMissingRequiredItems(tempWorld.HyruleCastle.BackOfEscape.DarkCross, progression);
            missingItems.Should().HaveCount(2);
            missingItems.Should().ContainEquivalentOf(new[] { ItemType.Lamp });
            missingItems.Should().ContainEquivalentOf(new[] { ItemType.Firerod });

            progression = new Progression(new[] { ItemType.Firerod });
            missingItems = Logic.GetMissingRequiredItems(tempWorld.HyruleCastle.BackOfEscape.DarkCross, progression);
            missingItems.Should().BeEmpty();
        }

        [Fact]
        public void TestInfiniteBombJump()
        {
            Config config = new Config();

            config.LogicConfig.InfiniteBombJump = false;
            World tempWorld = new World(config, "", 0, "");
            var progression = new Progression(new[] { ItemType.Morph, ItemType.PowerBomb, ItemType.Bombs });
            var missingItems = Logic.GetMissingRequiredItems(tempWorld.CentralCrateria.PowerBombRoom, progression);
            missingItems.Should().HaveCount(2);
            missingItems.Should().ContainEquivalentOf(new[] { ItemType.SpaceJump });
            missingItems.Should().ContainEquivalentOf(new[] { ItemType.SpeedBooster });

            config.LogicConfig.InfiniteBombJump = true;
            tempWorld = new World(config, "", 0, "");
            progression = new Progression(new[] { ItemType.Morph, ItemType.PowerBomb, ItemType.Bombs });
            missingItems = Logic.GetMissingRequiredItems(tempWorld.CentralCrateria.PowerBombRoom, progression);
            missingItems.Should().BeEmpty();
        }

        [Fact]
        public void TestParlorSpeedBooster()
        {
            Config config = new Config();

            config.LogicConfig.ParlorSpeedBooster = false;
            World tempWorld = new World(config, "", 0, "");
            var progression = new Progression();
            var missingItems = Logic.GetMissingRequiredItems(tempWorld.WestCrateria.Terminator, progression);
            missingItems.Should().HaveCount(7);
            missingItems.Should().NotContainEquivalentOf(new[] { ItemType.SpeedBooster });

            config.LogicConfig.ParlorSpeedBooster = true;
            tempWorld = new World(config, "", 0, "");
            progression = new Progression();
            missingItems = Logic.GetMissingRequiredItems(tempWorld.WestCrateria.Terminator, progression);
            missingItems.Should().HaveCount(8);
            missingItems.Should().ContainEquivalentOf(new[] { ItemType.SpeedBooster });

            progression = new Progression(new[] { ItemType.SpeedBooster });
            missingItems = Logic.GetMissingRequiredItems(tempWorld.WestCrateria.Terminator, progression);
            missingItems.Should().BeEmpty();
        }
    }
}
