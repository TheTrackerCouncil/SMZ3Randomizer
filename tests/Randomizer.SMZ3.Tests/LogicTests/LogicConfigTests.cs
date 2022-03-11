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
            missingItems.Should().HaveCount(1)
                .And.ContainEquivalentOf(new[] { ItemType.Morph });
        }

        [Fact]
        public void TestRequireTwoPowerBombs()
        {
            Config config = new Config();

            config.LogicConfig.PreventFivePowerBombSeed = false;
            World tempWorld = new World(config, "", 0, "");
            var progression = new Progression(new[] { ItemType.Morph, ItemType.PowerBomb });
            var missingItems = Logic.GetMissingRequiredItems(tempWorld.WestCrateria.Terminator, progression);
            missingItems.Should().BeEmpty();

            config.LogicConfig.PreventFivePowerBombSeed = true;
            tempWorld = new World(config, "", 0, "");
            progression = new Progression(new[] { ItemType.Morph, ItemType.PowerBomb });
            missingItems = Logic.GetMissingRequiredItems(tempWorld.WestCrateria.Terminator, progression);
            missingItems.Should().HaveCount(3)
                .And.ContainEquivalentOf(new[] { ItemType.Bombs })
                .And.ContainEquivalentOf(new[] { ItemType.ScrewAttack })
                .And.ContainEquivalentOf(new[] { ItemType.PowerBomb });

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
            missingItems.Should().HaveCount(1)
                .And.ContainEquivalentOf(new[] { ItemType.Lamp });

            config.LogicConfig.FireRodDarkRooms = true;
            tempWorld = new World(config, "", 0, "");
            progression = new Progression();
            missingItems = Logic.GetMissingRequiredItems(tempWorld.HyruleCastle.BackOfEscape.DarkCross, progression);
            missingItems.Should().HaveCount(2)
                .And.ContainEquivalentOf(new[] { ItemType.Lamp })
                .And.ContainEquivalentOf(new[] { ItemType.Firerod });

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
            missingItems.Should().HaveCount(2)
                .And.ContainEquivalentOf(new[] { ItemType.SpaceJump })
                .And.ContainEquivalentOf(new[] { ItemType.SpeedBooster });

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
            missingItems.Should().HaveCount(7)
                .And.NotContainEquivalentOf(new[] { ItemType.SpeedBooster });

            config.LogicConfig.ParlorSpeedBooster = true;
            tempWorld = new World(config, "", 0, "");
            progression = new Progression();
            missingItems = Logic.GetMissingRequiredItems(tempWorld.WestCrateria.Terminator, progression);
            missingItems.Should().HaveCount(8)
                .And.ContainEquivalentOf(new[] { ItemType.SpeedBooster });

            progression = new Progression(new[] { ItemType.SpeedBooster });
            missingItems = Logic.GetMissingRequiredItems(tempWorld.WestCrateria.Terminator, progression);
            missingItems.Should().BeEmpty();
        }

        [Fact]
        public void TestMockBall()
        {
            Config config = new Config();

            config.LogicConfig.MockBall = false;
            World tempWorld = new World(config, "", 0, "");
            var progression = new Progression(new[] { ItemType.Morph, ItemType.Bombs , ItemType.Missile , ItemType.PowerBomb, ItemType.ScrewAttack });
            var missingItems = Logic.GetMissingRequiredItems(tempWorld.GreenBrinstar.TopSuperMissile, progression);
            missingItems.Should().HaveCount(1)
                .And.ContainEquivalentOf(new[] { ItemType.SpeedBooster });

            config.LogicConfig.MockBall = true;
            tempWorld = new World(config, "", 0, "");
            missingItems = Logic.GetMissingRequiredItems(tempWorld.GreenBrinstar.TopSuperMissile, progression);
            missingItems.Should().BeEmpty();

            progression = new Progression(new[] { ItemType.Bombs, ItemType.Missile, ItemType.PowerBomb, ItemType.ScrewAttack });
            missingItems = Logic.GetMissingRequiredItems(tempWorld.GreenBrinstar.TopSuperMissile, progression);
            missingItems.Should().HaveCount(2)
                .And.ContainEquivalentOf(new[] { ItemType.SpeedBooster })
                .And.ContainEquivalentOf(new[] { ItemType.Morph });
        }

        [Fact]
        public void TestSwordOnlyDarkRoom()
        {
            Config config = new Config();

            config.LogicConfig.SwordOnlyDarkRooms = false;
            World tempWorld = new World(config, "", 0, "");
            var progression = new Progression(new[] { ItemType.ProgressiveSword });
            var missingItems = Logic.GetMissingRequiredItems(tempWorld.EasternPalace.BigKeyChest, progression);
            missingItems.Should().HaveCount(1)
                .And.ContainEquivalentOf(new[] { ItemType.Lamp });

            config.LogicConfig.SwordOnlyDarkRooms = true;
            tempWorld = new World(config, "", 0, "");
            missingItems = Logic.GetMissingRequiredItems(tempWorld.EasternPalace.BigKeyChest, progression);
            missingItems.Should().BeEmpty();

            progression = new Progression();
            missingItems = Logic.GetMissingRequiredItems(tempWorld.EasternPalace.BigKeyChest, progression);
            missingItems.Should().HaveCount(3)
                .And.ContainEquivalentOf(new[] { ItemType.ProgressiveSword })
                .And.ContainEquivalentOf(new[] { ItemType.Lamp });
        }

        [Fact]
        public void TestLightWorldSouthFakeFlippers()
        {
            Config config = new Config();

            config.LogicConfig.LightWorldSouthFakeFlippers = false;
            var tempWorld = new World(config, "", 0, "");
            var progression = new Progression();
            var missingItems = Logic.GetMissingRequiredItems(tempWorld.LightWorldSouth.UnderTheBridge, progression);
            missingItems.Should().HaveCount(1)
                .And.ContainEquivalentOf(new[] { ItemType.Flippers });
            missingItems = Logic.GetMissingRequiredItems(tempWorld.LightWorldNorthEast.WaterfallFairy.Locations.First(), progression);
            missingItems.Should().HaveCount(1)
                .And.ContainEquivalentOf(new[] { ItemType.Flippers });

            config.LogicConfig.LightWorldSouthFakeFlippers = true;
            tempWorld = new World(config, "", 0, "");
            missingItems = Logic.GetMissingRequiredItems(tempWorld.LightWorldSouth.UnderTheBridge, progression);
            missingItems.Should().BeEmpty();
            missingItems = Logic.GetMissingRequiredItems(tempWorld.LightWorldNorthEast.WaterfallFairy.Locations.First(), progression);
            missingItems.Should().HaveCount(2)
                .And.ContainEquivalentOf(new[] { ItemType.Flippers })
                .And.ContainEquivalentOf(new[] { ItemType.MoonPearl });

            progression = new Progression(new[] { ItemType.MoonPearl });
            missingItems = Logic.GetMissingRequiredItems(tempWorld.LightWorldNorthEast.WaterfallFairy.Locations.First(), progression);
            missingItems.Should().BeEmpty();
        }
    }
}
