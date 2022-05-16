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
            tempWorld.WestCrateria.Terminator.IsAvailable(progression).Should().BeTrue();

            config.LogicConfig.PreventScrewAttackSoftLock = true;
            tempWorld = new World(config, "", 0, "");
            progression = new Progression(new[] { ItemType.ScrewAttack });
            missingItems = Logic.GetMissingRequiredItems(tempWorld.WestCrateria.Terminator, progression);
            missingItems.Should().HaveCount(1)
                .And.ContainEquivalentOf(new[] { ItemType.Morph });
            tempWorld.WestCrateria.Terminator.IsAvailable(progression).Should().BeFalse();
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
            tempWorld.WestCrateria.Terminator.IsAvailable(progression).Should().BeTrue();

            config.LogicConfig.PreventFivePowerBombSeed = true;
            tempWorld = new World(config, "", 0, "");
            progression = new Progression(new[] { ItemType.Morph, ItemType.PowerBomb });
            missingItems = Logic.GetMissingRequiredItems(tempWorld.WestCrateria.Terminator, progression);
            missingItems.Should().HaveCount(3)
                .And.ContainEquivalentOf(new[] { ItemType.Bombs })
                .And.ContainEquivalentOf(new[] { ItemType.ScrewAttack })
                .And.ContainEquivalentOf(new[] { ItemType.PowerBomb });
            tempWorld.WestCrateria.Terminator.IsAvailable(progression).Should().BeFalse();

            progression = new Progression(new[] { ItemType.Morph, ItemType.PowerBomb, ItemType.PowerBomb });
            missingItems = Logic.GetMissingRequiredItems(tempWorld.WestCrateria.Terminator, progression);
            missingItems.Should().BeEmpty();
            tempWorld.WestCrateria.Terminator.IsAvailable(progression).Should().BeTrue();
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
            tempWorld.HyruleCastle.BackOfEscape.DarkCross.IsAvailable(progression).Should().BeFalse();

            config.LogicConfig.FireRodDarkRooms = true;
            tempWorld = new World(config, "", 0, "");
            progression = new Progression();
            missingItems = Logic.GetMissingRequiredItems(tempWorld.HyruleCastle.BackOfEscape.DarkCross, progression);
            missingItems.Should().HaveCount(2)
                .And.ContainEquivalentOf(new[] { ItemType.Lamp })
                .And.ContainEquivalentOf(new[] { ItemType.Firerod });
            tempWorld.HyruleCastle.BackOfEscape.DarkCross.IsAvailable(progression).Should().BeFalse();

            progression = new Progression(new[] { ItemType.Firerod });
            missingItems = Logic.GetMissingRequiredItems(tempWorld.HyruleCastle.BackOfEscape.DarkCross, progression);
            missingItems.Should().BeEmpty();
            tempWorld.HyruleCastle.BackOfEscape.DarkCross.IsAvailable(progression).Should().BeTrue();
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
            tempWorld.CentralCrateria.PowerBombRoom.IsAvailable(progression).Should().BeFalse();

            config.LogicConfig.InfiniteBombJump = true;
            tempWorld = new World(config, "", 0, "");
            progression = new Progression(new[] { ItemType.Morph, ItemType.PowerBomb, ItemType.Bombs });
            missingItems = Logic.GetMissingRequiredItems(tempWorld.CentralCrateria.PowerBombRoom, progression);
            missingItems.Should().BeEmpty();
            tempWorld.CentralCrateria.PowerBombRoom.IsAvailable(progression).Should().BeTrue();
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
            tempWorld.WestCrateria.Terminator.IsAvailable(progression).Should().BeFalse();

            config.LogicConfig.ParlorSpeedBooster = true;
            tempWorld = new World(config, "", 0, "");
            progression = new Progression();
            missingItems = Logic.GetMissingRequiredItems(tempWorld.WestCrateria.Terminator, progression);
            missingItems.Should().HaveCount(8)
                .And.ContainEquivalentOf(new[] { ItemType.SpeedBooster });
            tempWorld.WestCrateria.Terminator.IsAvailable(progression).Should().BeFalse();

            progression = new Progression(new[] { ItemType.SpeedBooster });
            missingItems = Logic.GetMissingRequiredItems(tempWorld.WestCrateria.Terminator, progression);
            missingItems.Should().BeEmpty();
            tempWorld.WestCrateria.Terminator.IsAvailable(progression).Should().BeTrue();
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
            tempWorld.GreenBrinstar.TopSuperMissile.IsAvailable(progression).Should().BeFalse();

            config.LogicConfig.MockBall = true;
            tempWorld = new World(config, "", 0, "");
            missingItems = Logic.GetMissingRequiredItems(tempWorld.GreenBrinstar.TopSuperMissile, progression);
            missingItems.Should().BeEmpty();
            tempWorld.GreenBrinstar.TopSuperMissile.IsAvailable(progression).Should().BeTrue();

            progression = new Progression(new[] { ItemType.Bombs, ItemType.Missile, ItemType.PowerBomb, ItemType.ScrewAttack });
            missingItems = Logic.GetMissingRequiredItems(tempWorld.GreenBrinstar.TopSuperMissile, progression);
            missingItems.Should().HaveCount(2)
                .And.ContainEquivalentOf(new[] { ItemType.SpeedBooster })
                .And.ContainEquivalentOf(new[] { ItemType.Morph });
            tempWorld.GreenBrinstar.TopSuperMissile.IsAvailable(progression).Should().BeFalse();
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
            tempWorld.EasternPalace.BigKeyChest.IsAvailable(progression).Should().BeFalse();

            config.LogicConfig.SwordOnlyDarkRooms = true;
            tempWorld = new World(config, "", 0, "");
            missingItems = Logic.GetMissingRequiredItems(tempWorld.EasternPalace.BigKeyChest, progression);
            missingItems.Should().BeEmpty();
            tempWorld.EasternPalace.BigKeyChest.IsAvailable(progression).Should().BeTrue();

            progression = new Progression();
            missingItems = Logic.GetMissingRequiredItems(tempWorld.EasternPalace.BigKeyChest, progression);
            missingItems.Should().HaveCount(3)
                .And.ContainEquivalentOf(new[] { ItemType.ProgressiveSword })
                .And.ContainEquivalentOf(new[] { ItemType.Lamp });
            tempWorld.EasternPalace.BigKeyChest.IsAvailable(progression).Should().BeFalse();
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
            tempWorld.LightWorldNorthEast.WaterfallFairy.Locations.First().IsAvailable(progression).Should().BeFalse();

            config.LogicConfig.LightWorldSouthFakeFlippers = true;
            tempWorld = new World(config, "", 0, "");
            missingItems = Logic.GetMissingRequiredItems(tempWorld.LightWorldSouth.UnderTheBridge, progression);
            missingItems.Should().BeEmpty();
            missingItems = Logic.GetMissingRequiredItems(tempWorld.LightWorldNorthEast.WaterfallFairy.Locations.First(), progression);
            missingItems.Should().HaveCount(2)
                .And.ContainEquivalentOf(new[] { ItemType.Flippers })
                .And.ContainEquivalentOf(new[] { ItemType.MoonPearl });
            tempWorld.LightWorldNorthEast.WaterfallFairy.Locations.First().IsAvailable(progression).Should().BeFalse();

            progression = new Progression(new[] { ItemType.MoonPearl });
            missingItems = Logic.GetMissingRequiredItems(tempWorld.LightWorldNorthEast.WaterfallFairy.Locations.First(), progression);
            missingItems.Should().BeEmpty();
            tempWorld.LightWorldNorthEast.WaterfallFairy.Locations.First().IsAvailable(progression).Should().BeTrue();
        }

        [Fact]
        public void TestLeftSandPitRequiresSpringBall()
        {
            Config config = new Config();

            config.LogicConfig.LeftSandPitRequiresSpringBall = false;
            var tempWorld = new World(config, "", 0, "");
            var progression = new Progression(new [] { ItemType.CardMaridiaL1, ItemType.Morph, ItemType.Super, ItemType.PowerBomb, ItemType.Gravity, ItemType.SpaceJump });
            var missingItems = Logic.GetMissingRequiredItems(tempWorld.InnerMaridia.LeftSandPit.Left, progression);
            missingItems.Should().BeEmpty();
            tempWorld.InnerMaridia.LeftSandPit.Left.IsAvailable(progression).Should().BeTrue();
            tempWorld.InnerMaridia.LeftSandPit.Right.IsAvailable(progression).Should().BeTrue();

            config.LogicConfig.LeftSandPitRequiresSpringBall = true;
            tempWorld = new World(config, "", 0, "");
            missingItems = Logic.GetMissingRequiredItems(tempWorld.InnerMaridia.LeftSandPit.Left, progression);
            missingItems.Should().HaveCount(1)
                .And.ContainEquivalentOf(new[] { ItemType.SpringBall });
            tempWorld.InnerMaridia.LeftSandPit.Left.IsAvailable(progression).Should().BeFalse();
            tempWorld.InnerMaridia.LeftSandPit.Right.IsAvailable(progression).Should().BeFalse();

            progression = new Progression(new[] { ItemType.CardMaridiaL1, ItemType.Morph, ItemType.Super, ItemType.PowerBomb, ItemType.Gravity, ItemType.SpaceJump, ItemType.SpringBall });
            missingItems = Logic.GetMissingRequiredItems(tempWorld.InnerMaridia.LeftSandPit.Left, progression);
            missingItems.Should().BeEmpty();
            tempWorld.InnerMaridia.LeftSandPit.Left.IsAvailable(progression).Should().BeTrue();
            tempWorld.InnerMaridia.LeftSandPit.Right.IsAvailable(progression).Should().BeTrue();
        }
    }
}
