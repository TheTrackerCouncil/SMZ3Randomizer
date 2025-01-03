using System.Collections.Generic;
using FluentAssertions;
using TrackerCouncil.Smz3.Data.Logic;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.Shared.Enums;
using Xunit;

namespace TrackerCouncil.Smz3.Tests.LogicTests;

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
        World.HyruleCastle.LinksUncle.UpdateAccessibility(emptyProgression, emptyProgression);
        var missingItems = Logic.GetMissingRequiredItems(World.HyruleCastle.LinksUncle, emptyProgression, out _);
        missingItems.Should().BeEmpty();
    }

    [Fact]
    public void LocationWithSatisfiedLogicHasNoMissingItems()
    {
        var progression = new Progression(new[] { new Item(ItemType.Boots, World) }, new List<Reward>(), new List<Boss>());
        World.LightWorldSouth.Library.UpdateAccessibility(progression, progression);
        var missingItems = Logic.GetMissingRequiredItems(World.LightWorldSouth.Library, progression, out _);
        missingItems.Should().BeEmpty();
    }

    [Fact]
    public void LocationWithSimpleLogicOnlyHasOneSetOfItems()
    {
        var emptyProgression = new Progression();
        var missingItems = Logic.GetMissingRequiredItems(World.LightWorldSouth.Library, emptyProgression, out _);
        missingItems.Should().HaveCount(1);
    }

    [Fact]
    public void LocationWithSimpleLogicReturnsSingleMissingItem()
    {
        var emptyProgression = new Progression();
        var missingItems = Logic.GetMissingRequiredItems(World.LightWorldSouth.Library, emptyProgression, out _);
        missingItems.Should().ContainEquivalentOf(new[] { ItemType.Boots });
    }

    [Fact]
    public void LocationWithTwoMissingItemsReturnsTwoMissingItems()
    {
        var emptyProgression = new Progression(World.ItemPools.Keycards, new List<Reward>(), new List<Boss>());
        var missingItems = Logic.GetMissingRequiredItems(World.FindLocation(LocationId.GreenBrinstarMainShaft), emptyProgression, out _);
        missingItems.Should().ContainEquivalentOf(new[] { ItemType.Morph, ItemType.PowerBomb });
    }

    [Fact]
    public void LocationWithMultipleOptionsReturnsAllOptions()
    {
        var emptyProgression = new Progression(World.ItemPools.Keycards, new List<Reward>(), new List<Boss>());
        var missingItems = Logic.GetMissingRequiredItems(World.FindLocation(LocationId.BlueBrinstarEnergyTankCeiling), emptyProgression, out _);
        missingItems.Should().ContainEquivalentOf(new[] { ItemType.SpaceJump })
            .And.ContainEquivalentOf(new[] { ItemType.HiJump })
            .And.ContainEquivalentOf(new[] { ItemType.SpeedBooster })
            .And.ContainEquivalentOf(new[] { ItemType.Ice });
    }

    [Fact]
    public void LocationWithThreeMissingItemsReturnsThreeMissingItems()
    {
        var emptyProgression = new Progression(World.ItemPools.Keycards, new List<Reward>(), new List<Boss>());
        var missingItems = Logic.GetMissingRequiredItems(World.FindLocation(LocationId.CrateriaBombTorizo), emptyProgression, out _);
        missingItems.Should().ContainEquivalentOf(new[] { ItemType.Morph, ItemType.Super, ItemType.PowerBomb });
    }

    [Fact]
    public void HiJumpLobbyMissilePreventsDeadEnd()
    {
        var progression = new Progression(new[] { ItemType.Flute, ItemType.Morph, ItemType.Missile }, new List<RewardType>(), new List<BossType>());
        var missingItems = Logic.GetMissingRequiredItems(World.FindLocation(LocationId.UpperNorfairHiJumpEnergyTankLeft), progression, out _);
        missingItems.Should().ContainEquivalentOf(new[] { ItemType.Bombs });
    }
}
