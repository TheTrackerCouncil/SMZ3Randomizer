using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using FluentAssertions;
using Randomizer.Data.Logic;
using Randomizer.Data.Options;
using Randomizer.Data.WorldData;
using Randomizer.Shared;
using Randomizer.Shared.Enums;
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
                category.Should().BeOneOf("Logic", "Tricks", "Patches");
            }
        }

        [Fact]
        public void TestPreventScrewAttackSoftLock()
        {
            Config config = new Config();

            config.LogicConfig.PreventScrewAttackSoftLock = false;
            World tempWorld = new World(config, "", 0, "");
            var progression = new Progression(new[] { ItemType.ScrewAttack }, new List<RewardType>(), new List<BossType>());
            var missingItems = Logic.GetMissingRequiredItems(tempWorld.WestCrateria.Terminator, progression, out _);
            missingItems.Should().BeEmpty();
            tempWorld.WestCrateria.Terminator.IsAvailable(progression).Should().BeTrue();

            config.LogicConfig.PreventScrewAttackSoftLock = true;
            tempWorld = new World(config, "", 0, "");
            progression = new Progression(new[] { ItemType.ScrewAttack }, new List<RewardType>(), new List<BossType>());
            missingItems = Logic.GetMissingRequiredItems(tempWorld.WestCrateria.Terminator, progression, out _);
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
            var progression = new Progression(new[] { ItemType.Morph, ItemType.PowerBomb }, new List<RewardType>(), new List<BossType>());
            var missingItems = Logic.GetMissingRequiredItems(tempWorld.WestCrateria.Terminator, progression, out _);
            missingItems.Should().BeEmpty();
            tempWorld.WestCrateria.Terminator.IsAvailable(progression).Should().BeTrue();

            config.LogicConfig.PreventFivePowerBombSeed = true;
            tempWorld = new World(config, "", 0, "");
            progression = new Progression(new[] { ItemType.Morph, ItemType.PowerBomb }, new List<RewardType>(), new List<BossType>());
            missingItems = Logic.GetMissingRequiredItems(tempWorld.WestCrateria.Terminator, progression, out _);
            missingItems.Should().HaveCount(3)
                .And.ContainEquivalentOf(new[] { ItemType.Bombs })
                .And.ContainEquivalentOf(new[] { ItemType.ScrewAttack })
                .And.ContainEquivalentOf(new[] { ItemType.PowerBomb });
            tempWorld.WestCrateria.Terminator.IsAvailable(progression).Should().BeFalse();

            progression = new Progression(new[] { ItemType.Morph, ItemType.PowerBomb, ItemType.PowerBomb }, new List<RewardType>(), new List<BossType>());
            missingItems = Logic.GetMissingRequiredItems(tempWorld.WestCrateria.Terminator, progression, out _);
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
            var missingItems = Logic.GetMissingRequiredItems(tempWorld.HyruleCastle.BackOfEscape.DarkCross, progression, out _);
            missingItems.Should().HaveCount(1)
                .And.ContainEquivalentOf(new[] { ItemType.Lamp });
            tempWorld.HyruleCastle.BackOfEscape.DarkCross.IsAvailable(progression).Should().BeFalse();

            config.LogicConfig.FireRodDarkRooms = true;
            tempWorld = new World(config, "", 0, "");
            progression = new Progression();
            missingItems = Logic.GetMissingRequiredItems(tempWorld.HyruleCastle.BackOfEscape.DarkCross, progression, out _);
            missingItems.Should().HaveCount(2)
                .And.ContainEquivalentOf(new[] { ItemType.Lamp })
                .And.ContainEquivalentOf(new[] { ItemType.Firerod });
            tempWorld.HyruleCastle.BackOfEscape.DarkCross.IsAvailable(progression).Should().BeFalse();

            progression = new Progression(new[] { ItemType.Firerod }, new List<RewardType>(), new List<BossType>());
            missingItems = Logic.GetMissingRequiredItems(tempWorld.HyruleCastle.BackOfEscape.DarkCross, progression, out _);
            missingItems.Should().BeEmpty();
            tempWorld.HyruleCastle.BackOfEscape.DarkCross.IsAvailable(progression).Should().BeTrue();
        }

        [Fact]
        public void TestInfiniteBombJump()
        {
            Config config = new Config();

            config.LogicConfig.InfiniteBombJump = false;
            World tempWorld = new World(config, "", 0, "");
            var progression = new Progression(new[] { ItemType.Morph, ItemType.PowerBomb, ItemType.Bombs }, new List<RewardType>(), new List<BossType>());
            var missingItems = Logic.GetMissingRequiredItems(tempWorld.CentralCrateria.PowerBombRoom, progression, out _);
            missingItems.Should().HaveCount(2)
                .And.ContainEquivalentOf(new[] { ItemType.SpaceJump })
                .And.ContainEquivalentOf(new[] { ItemType.SpeedBooster });
            tempWorld.CentralCrateria.PowerBombRoom.IsAvailable(progression).Should().BeFalse();

            config.LogicConfig.InfiniteBombJump = true;
            tempWorld = new World(config, "", 0, "");
            progression = new Progression(new[] { ItemType.Morph, ItemType.PowerBomb, ItemType.Bombs }, new List<RewardType>(), new List<BossType>());
            missingItems = Logic.GetMissingRequiredItems(tempWorld.CentralCrateria.PowerBombRoom, progression, out _);
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
            var missingItems = Logic.GetMissingRequiredItems(tempWorld.WestCrateria.Terminator, progression, out _);
            missingItems.Should().HaveCount(7)
                .And.NotContainEquivalentOf(new[] { ItemType.SpeedBooster });
            tempWorld.WestCrateria.Terminator.IsAvailable(progression).Should().BeFalse();

            config.LogicConfig.ParlorSpeedBooster = true;
            tempWorld = new World(config, "", 0, "");
            progression = new Progression();
            missingItems = Logic.GetMissingRequiredItems(tempWorld.WestCrateria.Terminator, progression, out _);
            missingItems.Should().HaveCount(8)
                .And.ContainEquivalentOf(new[] { ItemType.SpeedBooster });
            tempWorld.WestCrateria.Terminator.IsAvailable(progression).Should().BeFalse();

            progression = new Progression(new[] { ItemType.SpeedBooster }, new List<RewardType>(), new List<BossType>());
            missingItems = Logic.GetMissingRequiredItems(tempWorld.WestCrateria.Terminator, progression, out _);
            missingItems.Should().BeEmpty();
            tempWorld.WestCrateria.Terminator.IsAvailable(progression).Should().BeTrue();
        }

        [Fact]
        public void TestMockBall()
        {
            Config config = new Config();

            config.LogicConfig.MockBall = false;
            World tempWorld = new World(config, "", 0, "");
            var progression = new Progression(new[] { ItemType.Morph, ItemType.Bombs , ItemType.Missile , ItemType.PowerBomb, ItemType.ScrewAttack }, new List<RewardType>(), new List<BossType>());
            var missingItems = Logic.GetMissingRequiredItems(tempWorld.GreenBrinstar.TopSuperMissile, progression, out _);
            missingItems.Should().HaveCount(1)
                .And.ContainEquivalentOf(new[] { ItemType.SpeedBooster });
            tempWorld.GreenBrinstar.TopSuperMissile.IsAvailable(progression).Should().BeFalse();

            config.LogicConfig.MockBall = true;
            tempWorld = new World(config, "", 0, "");
            missingItems = Logic.GetMissingRequiredItems(tempWorld.GreenBrinstar.TopSuperMissile, progression, out _);
            missingItems.Should().BeEmpty();
            tempWorld.GreenBrinstar.TopSuperMissile.IsAvailable(progression).Should().BeTrue();

            progression = new Progression(new[] { ItemType.Bombs, ItemType.Missile, ItemType.PowerBomb, ItemType.ScrewAttack }, new List<RewardType>(), new List<BossType>());
            missingItems = Logic.GetMissingRequiredItems(tempWorld.GreenBrinstar.TopSuperMissile, progression, out _);
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
            var progression = new Progression(new[] { ItemType.ProgressiveSword }, new List<RewardType>(), new List<BossType>());
            var missingItems = Logic.GetMissingRequiredItems(tempWorld.EasternPalace.BigKeyChest, progression, out _);
            missingItems.Should().HaveCount(1)
                .And.ContainEquivalentOf(new[] { ItemType.Lamp });
            tempWorld.EasternPalace.BigKeyChest.IsAvailable(progression).Should().BeFalse();

            config.LogicConfig.SwordOnlyDarkRooms = true;
            tempWorld = new World(config, "", 0, "");
            missingItems = Logic.GetMissingRequiredItems(tempWorld.EasternPalace.BigKeyChest, progression, out _);
            missingItems.Should().BeEmpty();
            tempWorld.EasternPalace.BigKeyChest.IsAvailable(progression).Should().BeTrue();

            progression = new Progression();
            missingItems = Logic.GetMissingRequiredItems(tempWorld.EasternPalace.BigKeyChest, progression, out _);
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
            var missingItems = Logic.GetMissingRequiredItems(tempWorld.LightWorldSouth.UnderTheBridge, progression, out _);
            missingItems.Should().HaveCount(1)
                .And.ContainEquivalentOf(new[] { ItemType.Flippers });
            missingItems = Logic.GetMissingRequiredItems(tempWorld.LightWorldNorthEast.WaterfallFairy.Locations.First(), progression, out _);
            missingItems.Should().HaveCount(1)
                .And.ContainEquivalentOf(new[] { ItemType.Flippers });
            tempWorld.LightWorldNorthEast.WaterfallFairy.Locations.First().IsAvailable(progression).Should().BeFalse();

            config.LogicConfig.LightWorldSouthFakeFlippers = true;
            tempWorld = new World(config, "", 0, "");
            missingItems = Logic.GetMissingRequiredItems(tempWorld.LightWorldSouth.UnderTheBridge, progression, out _);
            missingItems.Should().BeEmpty();
            missingItems = Logic.GetMissingRequiredItems(tempWorld.LightWorldNorthEast.WaterfallFairy.Locations.First(), progression, out _);
            missingItems.Should().HaveCount(2)
                .And.ContainEquivalentOf(new[] { ItemType.Flippers })
                .And.ContainEquivalentOf(new[] { ItemType.MoonPearl });
            tempWorld.LightWorldNorthEast.WaterfallFairy.Locations.First().IsAvailable(progression).Should().BeFalse();

            progression = new Progression(new[] { ItemType.MoonPearl }, new List<RewardType>(), new List<BossType>());
            missingItems = Logic.GetMissingRequiredItems(tempWorld.LightWorldNorthEast.WaterfallFairy.Locations.First(), progression, out _);
            missingItems.Should().BeEmpty();
            tempWorld.LightWorldNorthEast.WaterfallFairy.Locations.First().IsAvailable(progression).Should().BeTrue();
        }

        [Fact]
        public void LeftSandPitRequiresWallJumpsWithoutSpringBallOption()
        {
            Config config = new Config();
            config.LogicConfig.WallJumpDifficulty = WallJumpDifficulty.Medium;
            config.LogicConfig.LeftSandPitRequiresSpringBall = false;

            var tempWorld = new World(config, "", 0, "");
            var progression = new Progression(new [] { ItemType.CardMaridiaL1, ItemType.Morph, ItemType.Super, ItemType.PowerBomb, ItemType.Gravity, ItemType.Grapple }, new List<RewardType>(), new List<BossType>());
            tempWorld.InnerMaridia.LeftSandPit.Left.IsAvailable(progression).Should().BeTrue();
            tempWorld.InnerMaridia.LeftSandPit.Right.IsAvailable(progression).Should().BeTrue();

            config.LogicConfig.WallJumpDifficulty = WallJumpDifficulty.None;
            tempWorld.InnerMaridia.LeftSandPit.Left.IsAvailable(progression).Should().BeFalse();
            tempWorld.InnerMaridia.LeftSandPit.Right.IsAvailable(progression).Should().BeFalse();

        }

        [Fact]
        public void LeftSandPitRequiresSpringBallIfConfigured()
        {
            Config config = new Config();
            config.LogicConfig.WallJumpDifficulty = WallJumpDifficulty.Medium;
            config.LogicConfig.LeftSandPitRequiresSpringBall = true;

            var tempWorld = new World(config, "", 0, "");
            var progression = new Progression(new[] { ItemType.CardMaridiaL1, ItemType.Morph, ItemType.Super, ItemType.PowerBomb, ItemType.Gravity, ItemType.SpaceJump }, new List<RewardType>(), new List<BossType>());
            var missingItems = Logic.GetMissingRequiredItems(tempWorld.InnerMaridia.LeftSandPit.Left, progression, out _);
            missingItems.Should().HaveCount(1)
                .And.ContainEquivalentOf(new[] { ItemType.SpringBall });
            tempWorld.InnerMaridia.LeftSandPit.Left.IsAvailable(progression).Should().BeFalse();
            tempWorld.InnerMaridia.LeftSandPit.Right.IsAvailable(progression).Should().BeFalse();

            progression = new Progression(new[] { ItemType.CardMaridiaL1, ItemType.Morph, ItemType.Super, ItemType.PowerBomb, ItemType.Gravity, ItemType.SpaceJump, ItemType.SpringBall, ItemType.HiJump }, new List<RewardType>(), new List<BossType>());
            missingItems = Logic.GetMissingRequiredItems(tempWorld.InnerMaridia.LeftSandPit.Left, progression, out _);
            missingItems.Should().BeEmpty();
            tempWorld.InnerMaridia.LeftSandPit.Left.IsAvailable(progression).Should().BeTrue();
            tempWorld.InnerMaridia.LeftSandPit.Right.IsAvailable(progression).Should().BeTrue();
        }

        [Fact]
        public void TestLaunchPadRequiresIceBeam()
        {
            Config config = new Config();

            config.LogicConfig.LaunchPadRequiresIceBeam = false;
            var tempWorld = new World(config, "", 0, "");
            var progression = new Progression(new[] { ItemType.ETank, ItemType.ETank, ItemType.Morph, ItemType.SpeedBooster, ItemType.PowerBomb, ItemType.PowerBomb}, new List<RewardType>(), new List<BossType>());
            var missingItems = Logic.GetMissingRequiredItems(tempWorld.CentralCrateria.SuperMissile, progression, out _);
            missingItems.Should().BeEmpty();
            tempWorld.CentralCrateria.SuperMissile.IsAvailable(progression).Should().BeTrue();

            config.LogicConfig.LaunchPadRequiresIceBeam = true;
            tempWorld = new World(config, "", 0, "");
            missingItems = Logic.GetMissingRequiredItems(tempWorld.CentralCrateria.SuperMissile, progression, out _);
            missingItems.Should().HaveCount(1)
                .And.ContainEquivalentOf(new[] { ItemType.Ice });
            tempWorld.CentralCrateria.SuperMissile.IsAvailable(progression).Should().BeFalse();

            progression = new Progression(new[] { ItemType.ETank, ItemType.ETank, ItemType.Morph, ItemType.SpeedBooster, ItemType.PowerBomb, ItemType.PowerBomb, ItemType.Ice }, new List<RewardType>(), new List<BossType>());
            missingItems = Logic.GetMissingRequiredItems(tempWorld.CentralCrateria.SuperMissile, progression, out _);
            missingItems.Should().BeEmpty();
            tempWorld.CentralCrateria.SuperMissile.IsAvailable(progression).Should().BeTrue();
        }

        [Fact]
        public void TestWaterwayNeedsGravitySuit()
        {
            Config config = new Config();

            config.LogicConfig.WaterwayNeedsGravitySuit = false;
            var tempWorld = new World(config, "", 0, "");
            var progression = new Progression(new[] { ItemType.ETank, ItemType.ETank, ItemType.Morph, ItemType.SpeedBooster, ItemType.PowerBomb, ItemType.PowerBomb, ItemType.Missile }, new List<RewardType>(), new List<BossType>());
            var missingItems = Logic.GetMissingRequiredItems(tempWorld.PinkBrinstar.Waterway, progression, out _);
            missingItems.Should().BeEmpty();
            tempWorld.PinkBrinstar.Waterway.IsAvailable(progression).Should().BeTrue();

            config.LogicConfig.WaterwayNeedsGravitySuit = true;
            tempWorld = new World(config, "", 0, "");
            missingItems = Logic.GetMissingRequiredItems(tempWorld.PinkBrinstar.Waterway, progression, out _);
            missingItems.Should().HaveCount(1)
                .And.ContainEquivalentOf(new[] { ItemType.Gravity });
            tempWorld.PinkBrinstar.Waterway.IsAvailable(progression).Should().BeFalse();

            progression = new Progression(new[] { ItemType.ETank, ItemType.ETank, ItemType.Morph, ItemType.SpeedBooster, ItemType.PowerBomb, ItemType.PowerBomb, ItemType.Missile, ItemType.Gravity }, new List<RewardType>(), new List<BossType>());
            missingItems = Logic.GetMissingRequiredItems(tempWorld.CentralCrateria.SuperMissile, progression, out _);
            missingItems.Should().BeEmpty();
            tempWorld.PinkBrinstar.Waterway.IsAvailable(progression).Should().BeTrue();
        }

        [Fact]
        public void TestEasyEastCrateriaSkyItem()
        {
            Config config = new Config();

            config.LogicConfig.EasyEastCrateriaSkyItem = false;
            var tempWorld = new World(config, "", 0, "");
            var progression = new Progression(new[] { ItemType.ETank, ItemType.ETank, ItemType.Morph, ItemType.PowerBomb, ItemType.PowerBomb, ItemType.Super, ItemType.Gravity, ItemType.Grapple }, new List<RewardType>(), new List<BossType>() { BossType.Phantoon });
            var missingItems = Logic.GetMissingRequiredItems(tempWorld.EastCrateria.SkyMissile, progression, out _);
            missingItems.Should().BeEmpty();
            tempWorld.EastCrateria.SkyMissile.IsAvailable(progression).Should().BeTrue();

            config.LogicConfig.EasyEastCrateriaSkyItem = true;
            tempWorld = new World(config, "", 0, "");
            missingItems = Logic.GetMissingRequiredItems(tempWorld.EastCrateria.SkyMissile, progression, out _);
            missingItems.Should().HaveCount(2)
                .And.ContainEquivalentOf(new[] { ItemType.SpaceJump })
                .And.ContainEquivalentOf(new[] { ItemType.SpeedBooster });

            tempWorld.EastCrateria.SkyMissile.IsAvailable(progression).Should().BeFalse();

            progression = new Progression(new[] { ItemType.ETank, ItemType.ETank, ItemType.Morph, ItemType.PowerBomb, ItemType.PowerBomb, ItemType.Super, ItemType.Gravity, ItemType.Grapple, ItemType.SpaceJump }, new List<RewardType>(), new List<BossType>() { BossType.Phantoon });
            missingItems = Logic.GetMissingRequiredItems(tempWorld.EastCrateria.SkyMissile, progression, out _);
            missingItems.Should().BeEmpty();
            tempWorld.EastCrateria.SkyMissile.IsAvailable(progression).Should().BeTrue();
        }
    }
}
