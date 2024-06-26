using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DynamicForms.Library.Core.Attributes;
using FluentAssertions;
using TrackerCouncil.Smz3.Data.Logic;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.Shared.Enums;
using Xunit;

namespace TrackerCouncil.Smz3.Tests.LogicTests;

public class LogicConfigTests
{
    [Fact]
    public void ValidateConfigDetails()
    {
        var config = new Config();
        var type = config.LogicConfig.GetType();

        foreach (var property in type.GetProperties().Where(x => x.SetMethod != null))
        {
            var attribute = property.GetCustomAttribute<DynamicFormObjectAttribute>();
            attribute.Should().NotBeNull($"Property {property.Name} of the LogicConfig should have a DynamicFormObjectAttribute associated with it");

            attribute?.GroupName.Should()
                .NotBeNullOrEmpty(
                    $"Property {property.Name}'s DynamicFormObjectATtribute should have a group associated with it");

            if (attribute is DynamicFormFieldCheckBoxAttribute checkBoxAttribute)
            {
                checkBoxAttribute.CheckBoxText.Should()
                    .NotBeNullOrEmpty($"Property {property.Name} should have CheckBoxText associated with it");
            }
            else if (attribute is DynamicFormFieldComboBoxAttribute comboBoxAttribute)
            {
                comboBoxAttribute.Label.Should()
                    .NotBeNullOrEmpty($"Property {property.Name} should have Label associated with it");
            }
        }
    }

    [Fact]
    public void TestPreventScrewAttackSoftLock()
    {
        Config config = new Config();

        config.LogicConfig.PreventScrewAttackSoftLock = false;
        World tempWorld = new World(config, "", 0, "");
        var progression = new Progression(new[] { ItemType.ScrewAttack }, new List<RewardType>(), new List<BossType>());
        var missingItems = Logic.GetMissingRequiredItems(tempWorld.FindLocation(LocationId.CrateriaTerminator), progression, out _);
        missingItems.Should().BeEmpty();
        tempWorld.FindLocation(LocationId.CrateriaTerminator).IsAvailable(progression).Should().BeTrue();

        config.LogicConfig.PreventScrewAttackSoftLock = true;
        tempWorld = new World(config, "", 0, "");
        progression = new Progression(new[] { ItemType.ScrewAttack }, new List<RewardType>(), new List<BossType>());
        missingItems = Logic.GetMissingRequiredItems(tempWorld.FindLocation(LocationId.CrateriaTerminator), progression, out _);
        missingItems.Should().HaveCount(1)
            .And.ContainEquivalentOf(new[] { ItemType.Morph });
        tempWorld.FindLocation(LocationId.CrateriaTerminator).IsAvailable(progression).Should().BeFalse();
    }

    [Fact]
    public void TestRequireTwoPowerBombs()
    {
        Config config = new Config();

        config.LogicConfig.PreventFivePowerBombSeed = false;
        World tempWorld = new World(config, "", 0, "");
        var progression = new Progression(new[] { ItemType.Morph, ItemType.PowerBomb }, new List<RewardType>(), new List<BossType>());
        var missingItems = Logic.GetMissingRequiredItems(tempWorld.FindLocation(LocationId.CrateriaTerminator), progression, out _);
        missingItems.Should().BeEmpty();
        tempWorld.FindLocation(LocationId.CrateriaTerminator).IsAvailable(progression).Should().BeTrue();

        config.LogicConfig.PreventFivePowerBombSeed = true;
        tempWorld = new World(config, "", 0, "");
        progression = new Progression(new[] { ItemType.Morph, ItemType.PowerBomb }, new List<RewardType>(), new List<BossType>());
        missingItems = Logic.GetMissingRequiredItems(tempWorld.FindLocation(LocationId.CrateriaTerminator), progression, out _);
        missingItems.Should().HaveCount(3)
            .And.ContainEquivalentOf(new[] { ItemType.Bombs })
            .And.ContainEquivalentOf(new[] { ItemType.ScrewAttack })
            .And.ContainEquivalentOf(new[] { ItemType.PowerBomb });
        tempWorld.FindLocation(LocationId.CrateriaTerminator).IsAvailable(progression).Should().BeFalse();

        progression = new Progression(new[] { ItemType.Morph, ItemType.PowerBomb, ItemType.PowerBomb }, new List<RewardType>(), new List<BossType>());
        missingItems = Logic.GetMissingRequiredItems(tempWorld.FindLocation(LocationId.CrateriaTerminator), progression, out _);
        missingItems.Should().BeEmpty();
        tempWorld.FindLocation(LocationId.CrateriaTerminator).IsAvailable(progression).Should().BeTrue();
    }

    [Fact]
    public void TestFireRodDarkRooms()
    {
        Config config = new Config();

        config.LogicConfig.FireRodDarkRooms = false;
        World tempWorld = new World(config, "", 0, "");
        var progression = new Progression();
        var missingItems = Logic.GetMissingRequiredItems(tempWorld.FindLocation(LocationId.SewersDarkCross), progression, out _);
        missingItems.Should().HaveCount(1)
            .And.ContainEquivalentOf(new[] { ItemType.Lamp });
        tempWorld.FindLocation(LocationId.SewersDarkCross).IsAvailable(progression).Should().BeFalse();

        config.LogicConfig.FireRodDarkRooms = true;
        tempWorld = new World(config, "", 0, "");
        progression = new Progression();
        missingItems = Logic.GetMissingRequiredItems(tempWorld.FindLocation(LocationId.SewersDarkCross), progression, out _);
        missingItems.Should().HaveCount(2)
            .And.ContainEquivalentOf(new[] { ItemType.Lamp })
            .And.ContainEquivalentOf(new[] { ItemType.Firerod });
        tempWorld.FindLocation(LocationId.SewersDarkCross).IsAvailable(progression).Should().BeFalse();

        progression = new Progression(new[] { ItemType.Firerod }, new List<RewardType>(), new List<BossType>());
        missingItems = Logic.GetMissingRequiredItems(tempWorld.FindLocation(LocationId.SewersDarkCross), progression, out _);
        missingItems.Should().BeEmpty();
        tempWorld.FindLocation(LocationId.SewersDarkCross).IsAvailable(progression).Should().BeTrue();
    }

    [Fact]
    public void TestInfiniteBombJump()
    {
        Config config = new Config();

        config.LogicConfig.InfiniteBombJump = false;
        World tempWorld = new World(config, "", 0, "");
        var progression = new Progression(new[] { ItemType.Morph, ItemType.PowerBomb, ItemType.Bombs }, new List<RewardType>(), new List<BossType>());
        var missingItems = Logic.GetMissingRequiredItems(tempWorld.FindLocation(LocationId.CrateriaPowerBomb), progression, out _);
        missingItems.Should().HaveCount(2)
            .And.ContainEquivalentOf(new[] { ItemType.SpaceJump })
            .And.ContainEquivalentOf(new[] { ItemType.SpeedBooster });
        tempWorld.FindLocation(LocationId.CrateriaPowerBomb).IsAvailable(progression).Should().BeFalse();

        config.LogicConfig.InfiniteBombJump = true;
        tempWorld = new World(config, "", 0, "");
        progression = new Progression(new[] { ItemType.Morph, ItemType.PowerBomb, ItemType.Bombs }, new List<RewardType>(), new List<BossType>());
        missingItems = Logic.GetMissingRequiredItems(tempWorld.FindLocation(LocationId.CrateriaPowerBomb), progression, out _);
        missingItems.Should().BeEmpty();
        tempWorld.FindLocation(LocationId.CrateriaPowerBomb).IsAvailable(progression).Should().BeTrue();
    }

    [Fact]
    public void TestParlorSpeedBooster()
    {
        Config config = new Config();

        config.LogicConfig.ParlorSpeedBooster = false;
        World tempWorld = new World(config, "", 0, "");
        var progression = new Progression();
        var missingItems = Logic.GetMissingRequiredItems(tempWorld.FindLocation(LocationId.CrateriaTerminator), progression, out _);
        missingItems.Should().HaveCount(7)
            .And.NotContainEquivalentOf(new[] { ItemType.SpeedBooster });
        tempWorld.FindLocation(LocationId.CrateriaTerminator).IsAvailable(progression).Should().BeFalse();

        config.LogicConfig.ParlorSpeedBooster = true;
        tempWorld = new World(config, "", 0, "");
        progression = new Progression();
        missingItems = Logic.GetMissingRequiredItems(tempWorld.FindLocation(LocationId.CrateriaTerminator), progression, out _);
        missingItems.Should().HaveCount(8)
            .And.ContainEquivalentOf(new[] { ItemType.SpeedBooster });
        tempWorld.FindLocation(LocationId.CrateriaTerminator).IsAvailable(progression).Should().BeFalse();

        progression = new Progression(new[] { ItemType.SpeedBooster }, new List<RewardType>(), new List<BossType>());
        missingItems = Logic.GetMissingRequiredItems(tempWorld.FindLocation(LocationId.CrateriaTerminator), progression, out _);
        missingItems.Should().BeEmpty();
        tempWorld.FindLocation(LocationId.CrateriaTerminator).IsAvailable(progression).Should().BeTrue();
    }

    [Fact]
    public void TestMockBall()
    {
        Config config = new Config();

        config.LogicConfig.MockBall = false;
        World tempWorld = new World(config, "", 0, "");
        var progression = new Progression(new[] { ItemType.Morph, ItemType.Bombs , ItemType.Missile , ItemType.PowerBomb, ItemType.ScrewAttack }, new List<RewardType>(), new List<BossType>());
        var missingItems = Logic.GetMissingRequiredItems(tempWorld.FindLocation(LocationId.GreenBrinstarEarlySupersTop), progression, out _);
        missingItems.Should().HaveCount(1)
            .And.ContainEquivalentOf(new[] { ItemType.SpeedBooster });
        tempWorld.FindLocation(LocationId.GreenBrinstarEarlySupersTop).IsAvailable(progression).Should().BeFalse();

        config.LogicConfig.MockBall = true;
        tempWorld = new World(config, "", 0, "");
        missingItems = Logic.GetMissingRequiredItems(tempWorld.FindLocation(LocationId.GreenBrinstarEarlySupersTop), progression, out _);
        missingItems.Should().BeEmpty();
        tempWorld.FindLocation(LocationId.GreenBrinstarEarlySupersTop).IsAvailable(progression).Should().BeTrue();

        progression = new Progression(new[] { ItemType.Bombs, ItemType.Missile, ItemType.PowerBomb, ItemType.ScrewAttack }, new List<RewardType>(), new List<BossType>());
        missingItems = Logic.GetMissingRequiredItems(tempWorld.FindLocation(LocationId.GreenBrinstarEarlySupersTop), progression, out _);
        missingItems.Should().HaveCount(2)
            .And.ContainEquivalentOf(new[] { ItemType.SpeedBooster })
            .And.ContainEquivalentOf(new[] { ItemType.Morph });
        tempWorld.FindLocation(LocationId.GreenBrinstarEarlySupersTop).IsAvailable(progression).Should().BeFalse();
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
        tempWorld.FindLocation(LocationId.InnerMaridiaWestSandHoleLeft).IsAvailable(progression).Should().BeTrue();
        tempWorld.FindLocation(LocationId.InnerMaridiaWestSandHoleRight).IsAvailable(progression).Should().BeTrue();

        config.LogicConfig.WallJumpDifficulty = WallJumpDifficulty.None;
        tempWorld.FindLocation(LocationId.InnerMaridiaWestSandHoleLeft).IsAvailable(progression).Should().BeFalse();
        tempWorld.FindLocation(LocationId.InnerMaridiaWestSandHoleRight).IsAvailable(progression).Should().BeFalse();

    }

    [Fact]
    public void LeftSandPitRequiresSpringBallIfConfigured()
    {
        Config config = new Config();
        config.LogicConfig.WallJumpDifficulty = WallJumpDifficulty.Medium;
        config.LogicConfig.LeftSandPitRequiresSpringBall = true;

        var tempWorld = new World(config, "", 0, "");
        var progression = new Progression(new[] { ItemType.CardMaridiaL1, ItemType.Morph, ItemType.Super, ItemType.PowerBomb, ItemType.Gravity, ItemType.SpaceJump }, new List<RewardType>(), new List<BossType>());
        var missingItems = Logic.GetMissingRequiredItems(tempWorld.FindLocation(LocationId.InnerMaridiaWestSandHoleLeft), progression, out _);
        missingItems.Should().HaveCount(1)
            .And.ContainEquivalentOf(new[] { ItemType.SpringBall });
        tempWorld.FindLocation(LocationId.InnerMaridiaWestSandHoleLeft).IsAvailable(progression).Should().BeFalse();
        tempWorld.FindLocation(LocationId.InnerMaridiaWestSandHoleRight).IsAvailable(progression).Should().BeFalse();

        progression = new Progression(new[] { ItemType.CardMaridiaL1, ItemType.Morph, ItemType.Super, ItemType.PowerBomb, ItemType.Gravity, ItemType.SpaceJump, ItemType.SpringBall, ItemType.HiJump }, new List<RewardType>(), new List<BossType>());
        missingItems = Logic.GetMissingRequiredItems(tempWorld.FindLocation(LocationId.InnerMaridiaWestSandHoleLeft), progression, out _);
        missingItems.Should().BeEmpty();
        tempWorld.FindLocation(LocationId.InnerMaridiaWestSandHoleLeft).IsAvailable(progression).Should().BeTrue();
        tempWorld.FindLocation(LocationId.InnerMaridiaWestSandHoleRight).IsAvailable(progression).Should().BeTrue();
    }

    [Fact]
    public void TestLaunchPadRequiresIceBeam()
    {
        Config config = new Config();

        config.LogicConfig.LaunchPadRequiresIceBeam = false;
        var tempWorld = new World(config, "", 0, "");
        var progression = new Progression(new[] { ItemType.ETank, ItemType.ETank, ItemType.Morph, ItemType.SpeedBooster, ItemType.PowerBomb, ItemType.PowerBomb}, new List<RewardType>(), new List<BossType>());
        var missingItems = Logic.GetMissingRequiredItems(tempWorld.FindLocation(LocationId.CrateriaSuper), progression, out _);
        missingItems.Should().BeEmpty();
        tempWorld.FindLocation(LocationId.CrateriaSuper).IsAvailable(progression).Should().BeTrue();

        config.LogicConfig.LaunchPadRequiresIceBeam = true;
        tempWorld = new World(config, "", 0, "");
        missingItems = Logic.GetMissingRequiredItems(tempWorld.FindLocation(LocationId.CrateriaSuper), progression, out _);
        missingItems.Should().HaveCount(1)
            .And.ContainEquivalentOf(new[] { ItemType.Ice });
        tempWorld.FindLocation(LocationId.CrateriaSuper).IsAvailable(progression).Should().BeFalse();

        progression = new Progression(new[] { ItemType.ETank, ItemType.ETank, ItemType.Morph, ItemType.SpeedBooster, ItemType.PowerBomb, ItemType.PowerBomb, ItemType.Ice }, new List<RewardType>(), new List<BossType>());
        missingItems = Logic.GetMissingRequiredItems(tempWorld.FindLocation(LocationId.CrateriaSuper), progression, out _);
        missingItems.Should().BeEmpty();
        tempWorld.FindLocation(LocationId.CrateriaSuper).IsAvailable(progression).Should().BeTrue();
    }

    [Fact]
    public void TestWaterwayNeedsGravitySuit()
    {
        Config config = new Config();

        config.LogicConfig.WaterwayNeedsGravitySuit = false;
        var tempWorld = new World(config, "", 0, "");
        var progression = new Progression(new[] { ItemType.ETank, ItemType.ETank, ItemType.Morph, ItemType.SpeedBooster, ItemType.PowerBomb, ItemType.PowerBomb, ItemType.Missile }, new List<RewardType>(), new List<BossType>());
        var missingItems = Logic.GetMissingRequiredItems(tempWorld.FindLocation(LocationId.PinkBrinstarWaterwayEnergyTank), progression, out _);
        missingItems.Should().BeEmpty();
        tempWorld.FindLocation(LocationId.PinkBrinstarWaterwayEnergyTank).IsAvailable(progression).Should().BeTrue();

        config.LogicConfig.WaterwayNeedsGravitySuit = true;
        tempWorld = new World(config, "", 0, "");
        missingItems = Logic.GetMissingRequiredItems(tempWorld.FindLocation(LocationId.PinkBrinstarWaterwayEnergyTank), progression, out _);
        missingItems.Should().HaveCount(1)
            .And.ContainEquivalentOf(new[] { ItemType.Gravity });
        tempWorld.FindLocation(LocationId.PinkBrinstarWaterwayEnergyTank).IsAvailable(progression).Should().BeFalse();

        progression = new Progression(new[] { ItemType.ETank, ItemType.ETank, ItemType.Morph, ItemType.SpeedBooster, ItemType.PowerBomb, ItemType.PowerBomb, ItemType.Missile, ItemType.Gravity }, new List<RewardType>(), new List<BossType>());
        missingItems = Logic.GetMissingRequiredItems(tempWorld.FindLocation(LocationId.CrateriaSuper), progression, out _);
        missingItems.Should().BeEmpty();
        tempWorld.FindLocation(LocationId.PinkBrinstarWaterwayEnergyTank).IsAvailable(progression).Should().BeTrue();
    }

    [Fact]
    public void TestEasyEastCrateriaSkyItem()
    {
        Config config = new Config();

        config.LogicConfig.EasyEastCrateriaSkyItem = false;
        var tempWorld = new World(config, "", 0, "");
        var progression = new Progression(new[] { ItemType.ETank, ItemType.ETank, ItemType.Morph, ItemType.PowerBomb, ItemType.PowerBomb, ItemType.Super, ItemType.Gravity, ItemType.Grapple }, new List<RewardType>(), new List<BossType>() { BossType.Phantoon });
        var missingItems = Logic.GetMissingRequiredItems(tempWorld.FindLocation(LocationId.CrateriaWestOceanSky), progression, out _);
        missingItems.Should().BeEmpty();
        tempWorld.FindLocation(LocationId.CrateriaWestOceanSky).IsAvailable(progression).Should().BeTrue();

        config.LogicConfig.EasyEastCrateriaSkyItem = true;
        tempWorld = new World(config, "", 0, "");
        missingItems = Logic.GetMissingRequiredItems(tempWorld.FindLocation(LocationId.CrateriaWestOceanSky), progression, out _);
        missingItems.Should().HaveCount(2)
            .And.ContainEquivalentOf(new[] { ItemType.SpaceJump })
            .And.ContainEquivalentOf(new[] { ItemType.SpeedBooster });

        tempWorld.FindLocation(LocationId.CrateriaWestOceanSky).IsAvailable(progression).Should().BeFalse();

        progression = new Progression(new[] { ItemType.ETank, ItemType.ETank, ItemType.Morph, ItemType.PowerBomb, ItemType.PowerBomb, ItemType.Super, ItemType.Gravity, ItemType.Grapple, ItemType.SpaceJump }, new List<RewardType>(), new List<BossType>() { BossType.Phantoon });
        missingItems = Logic.GetMissingRequiredItems(tempWorld.FindLocation(LocationId.CrateriaWestOceanSky), progression, out _);
        missingItems.Should().BeEmpty();
        tempWorld.FindLocation(LocationId.CrateriaWestOceanSky).IsAvailable(progression).Should().BeTrue();
    }

    [Fact]
    public void TestKholdstareNeedsCaneOfSomaria()
    {
        var config = new Config();

        var items = new List<ItemType>()
        {
            ItemType.MoonPearl,
            ItemType.Firerod,
            ItemType.Hookshot,
            ItemType.ProgressiveGlove,
            ItemType.ProgressiveGlove,
            ItemType.ProgressiveSword,
            ItemType.Flippers,
            ItemType.BigKeyIP,
            ItemType.Hammer,
            ItemType.KeyIP,
        };

        config.LogicConfig.KholdstareNeedsCaneOfSomaria = false;
        var tempWorld = new World(config, "", 0, "");
        var progression = new Progression(items, Array.Empty<RewardType>(), Array.Empty<BossType>());
        var missingItems = Logic.GetMissingRequiredItems(tempWorld.IcePalace.KholdstareReward, progression, out _);
        missingItems.Should().HaveCount(2)
            .And.ContainEquivalentOf(new[] { ItemType.KeyIP })
            .And.ContainEquivalentOf(new[] { ItemType.Somaria });

        progression = new Progression(items.Append(ItemType.KeyIP), Array.Empty<RewardType>(), Array.Empty<BossType>());
        missingItems = Logic.GetMissingRequiredItems(tempWorld.IcePalace.KholdstareReward, progression, out _);
        missingItems.Should().BeEmpty();

        config.LogicConfig.KholdstareNeedsCaneOfSomaria = true;
        tempWorld = new World(config, "", 0, "");
        progression = new Progression(items, Array.Empty<RewardType>(), Array.Empty<BossType>());
        missingItems = Logic.GetMissingRequiredItems(tempWorld.IcePalace.KholdstareReward, progression, out _);
        missingItems.Should().HaveCount(1)
            .And.ContainEquivalentOf(new[] { ItemType.Somaria });

        progression = new Progression(items.Append(ItemType.KeyIP), Array.Empty<RewardType>(), Array.Empty<BossType>());
        missingItems = Logic.GetMissingRequiredItems(tempWorld.IcePalace.KholdstareReward, progression, out _);
        missingItems.Should().HaveCount(1)
            .And.ContainEquivalentOf(new[] { ItemType.Somaria });

        progression = new Progression(items.Append(ItemType.Somaria), Array.Empty<RewardType>(), Array.Empty<BossType>());
        missingItems = Logic.GetMissingRequiredItems(tempWorld.IcePalace.KholdstareReward, progression, out _);
        missingItems.Should().BeEmpty();
    }

    [Fact]
    public void TestEasyBlueBrinstarTop()
    {
        var config = new Config { LogicConfig = { EasyBlueBrinstarTop = false, }, KeysanityMode = KeysanityMode.None };

        var tempWorld = new World(config, "", 0, "");
        var progression = new Progression(new[] { ItemType.Morph, ItemType.PowerBomb, ItemType.CardBrinstarL1 }, new List<RewardType>(), new List<BossType>());
        Logic.GetMissingRequiredItems(tempWorld.FindLocation(LocationId.BlueBrinstarDoubleMissileVisible), progression, out _).Should().BeEmpty();
        Logic.GetMissingRequiredItems(tempWorld.FindLocation(LocationId.BlueBrinstarDoubleMissileHidden), progression, out _).Should().BeEmpty();
        tempWorld.FindLocation(LocationId.BlueBrinstarDoubleMissileVisible).IsAvailable(progression).Should().BeTrue();
        tempWorld.FindLocation(LocationId.BlueBrinstarDoubleMissileHidden).IsAvailable(progression).Should().BeTrue();

        config.LogicConfig.EasyBlueBrinstarTop = true;
        tempWorld = new World(config, "", 0, "");
        Logic.GetMissingRequiredItems(tempWorld.FindLocation(LocationId.BlueBrinstarDoubleMissileVisible), progression, out _).Should().HaveCount(2)
            .And.ContainEquivalentOf(new[] { ItemType.SpaceJump })
            .And.ContainEquivalentOf(new[] { ItemType.Gravity });
        Logic.GetMissingRequiredItems(tempWorld.FindLocation(LocationId.BlueBrinstarDoubleMissileHidden), progression, out _).Should().HaveCount(2)
            .And.ContainEquivalentOf(new[] { ItemType.SpaceJump })
            .And.ContainEquivalentOf(new[] { ItemType.Gravity });
        tempWorld.FindLocation(LocationId.BlueBrinstarDoubleMissileVisible).IsAvailable(progression).Should().BeFalse();
        tempWorld.FindLocation(LocationId.BlueBrinstarDoubleMissileHidden).IsAvailable(progression).Should().BeFalse();

        progression = new Progression(new[] { ItemType.Morph, ItemType.PowerBomb, ItemType.Gravity, ItemType.CardBrinstarL1 }, new List<RewardType>(), new List<BossType>());
        Logic.GetMissingRequiredItems(tempWorld.FindLocation(LocationId.BlueBrinstarDoubleMissileVisible), progression, out _).Should().BeEmpty();
        Logic.GetMissingRequiredItems(tempWorld.FindLocation(LocationId.BlueBrinstarDoubleMissileHidden), progression, out _).Should().BeEmpty();
        tempWorld.FindLocation(LocationId.BlueBrinstarDoubleMissileVisible).IsAvailable(progression).Should().BeTrue();
        tempWorld.FindLocation(LocationId.BlueBrinstarDoubleMissileHidden).IsAvailable(progression).Should().BeTrue();

        progression = new Progression(new[] { ItemType.Morph, ItemType.PowerBomb, ItemType.SpaceJump, ItemType.CardBrinstarL1 }, new List<RewardType>(), new List<BossType>());
        Logic.GetMissingRequiredItems(tempWorld.FindLocation(LocationId.BlueBrinstarDoubleMissileVisible), progression, out _).Should().BeEmpty();
        Logic.GetMissingRequiredItems(tempWorld.FindLocation(LocationId.BlueBrinstarDoubleMissileHidden), progression, out _).Should().BeEmpty();
        tempWorld.FindLocation(LocationId.BlueBrinstarDoubleMissileVisible).IsAvailable(progression).Should().BeTrue();
        tempWorld.FindLocation(LocationId.BlueBrinstarDoubleMissileHidden).IsAvailable(progression).Should().BeTrue();
    }

    [Fact]
    public void TestZoraNeedsRupeeItems()
    {
        var config = new Config { LogicConfig = { ZoraNeedsRupeeItems = false, }};

        var tempWorld = new World(config, "", 0, "");
        var progression = new Progression(new[] { ItemType.Flippers, ItemType.ThreeHundredRupees}, new List<RewardType>(), new List<BossType>());
        Logic.GetMissingRequiredItems(tempWorld.FindLocation(LocationId.KingZora), progression, out _).Should().BeEmpty();
        tempWorld.FindLocation(LocationId.KingZora).IsAvailable(progression).Should().BeTrue();

        config.LogicConfig.ZoraNeedsRupeeItems = true;
        tempWorld = new World(config, "", 0, "");
        var items = Logic.GetMissingRequiredItems(tempWorld.FindLocation(LocationId.KingZora), progression,
            out _);
        Logic.GetMissingRequiredItems(tempWorld.FindLocation(LocationId.KingZora), progression, out _).Should()
            .HaveCount(1)
            .And.ContainEquivalentOf(new[] { ItemType.ThreeHundredRupees });
        tempWorld.FindLocation(LocationId.KingZora).IsAvailable(progression).Should().BeFalse();

        progression = new Progression(new[] { ItemType.Flippers, ItemType.ThreeHundredRupees, ItemType.ThreeHundredRupees }, new List<RewardType>(), new List<BossType>());
        Logic.GetMissingRequiredItems(tempWorld.FindLocation(LocationId.KingZora), progression, out _).Should().BeEmpty();
        tempWorld.FindLocation(LocationId.KingZora).IsAvailable(progression).Should().BeTrue();
    }
}
