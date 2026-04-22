using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Data.Configuration;
using TrackerCouncil.Smz3.Data.Logic;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Data.Services;
using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.Data.WorldData.Regions;
using TrackerCouncil.Smz3.Data.WorldData.Regions.Zelda;
using TrackerCouncil.Smz3.Data.WorldData.Regions.Zelda.DarkWorld;
using TrackerCouncil.Smz3.Data.WorldData.Regions.Zelda.LightWorld;
using TrackerCouncil.Smz3.SeedGenerator.Contracts;
using TrackerCouncil.Smz3.SeedGenerator.FileData.Patches;
using TrackerCouncil.Smz3.SeedGenerator.GameModes;
using TrackerCouncil.Smz3.SeedGenerator.Generation;
using TrackerCouncil.Smz3.SeedGenerator.Infrastructure;
using TrackerCouncil.Smz3.Shared.Enums;
using Xunit;

namespace TrackerCouncil.Smz3.Tests.Text;

public class GameHintTests
{

    [Fact]
    void BasicHints()
    {
        var world = GetVanillaWorld();
        var hintService = GetGameHintService();

        // Link's house (compass) should be mandatory
        var locations = world.Locations.Where(x => x.Id is LocationId.LinksHouse).ToList();
        Assert.Equal(LocationUsefulness.Mandatory, hintService.GetUsefulness(locations, [world], null));

        // Blind's hideout (where we put the morph bombs) should be mandatory
        locations = world.Locations.Where(x => x.Room is LightWorldNorthWest.BlindsHideoutRoom).ToList();
        Assert.Equal(LocationUsefulness.Mandatory, hintService.GetUsefulness(locations, [world], null));

        // Hype cave should be useless (only rupees)
        locations = world.Locations.Where(x => x.Room is DarkWorldSouth.HypeCaveRoom).ToList();
        Assert.Equal(LocationUsefulness.Useless, hintService.GetUsefulness(locations, [world], null));

        // Kraid's lair etank should be useless
        locations = world.Locations.Where(x => x.Id is LocationId.KraidsLairEnergyTank).ToList();
        Assert.Equal(LocationUsefulness.Useless, hintService.GetUsefulness(locations, [world], null));

        // Spazer should be nice to have
        locations = world.Locations.Where(x => x.Id is LocationId.RedBrinstarSpazer).ToList();
        Assert.Equal(LocationUsefulness.NiceToHave, hintService.GetUsefulness(locations, [world], null));

        // If we move the gloves out of desert palace, it should still be mandatory because we need the master sword from ped
        var desertGlove = world.FindLocation(LocationId.DesertPalaceBigChest);
        var desertGloveOtherItem = world.Locations.First(x => x is { Room: LightWorldNorthWest.BlindsHideoutRoom, ItemType: ItemType.ThreeBombs });
        SwapLocationItems(desertGlove, desertGloveOtherItem);
        locations = world.Locations.Where(x => x.Region is DesertPalace).ToList();
        Assert.Equal(LocationUsefulness.Useless, hintService.GetUsefulness(locations, [world], null));

        // ... but if we make a sword accessible elsewhere, it should be useless
        var pedestalSword = world.FindLocation(LocationId.MasterSwordPedestal);
        var pedestalSwordOtherItem = world.Locations.First(x => x is { Room: LightWorldNorthWest.BlindsHideoutRoom, ItemType: ItemType.ThreeBombs });
        SwapLocationItems(pedestalSword, pedestalSwordOtherItem);
        Assert.Equal(LocationUsefulness.Useless, hintService.GetUsefulness(locations, [world], null));

        // ... and if we move a different sword into desert palace, it should say that it's a sword instead of useless as there are two sword options
        var blackSmithSword = world.FindLocation(LocationId.Blacksmith);
        SwapLocationItems(desertGlove, blackSmithSword);
        Assert.Equal(LocationUsefulness.Sword, hintService.GetUsefulness(locations, [world], null));

        // ... but if it's the only option for the master sword, it should still count as mandatory for Aga access
        var morphBallLocation = world.FindLocation(LocationId.BlueBrinstarMorphBallRight);
        var pyramidLedge = world.FindLocation(LocationId.Pyramid);
        SwapLocationItems(desertGlove, blackSmithSword);
        SwapLocationItems(desertGlove, pedestalSwordOtherItem);
        SwapLocationItems(morphBallLocation, pyramidLedge);
        Assert.Equal(LocationUsefulness.Mandatory, hintService.GetUsefulness(locations, [world], null));
    }

    [Fact]
    void VanillaGoalZeldaHints()
    {
        var world = GetVanillaWorld();
        var hintService = GetGameHintService();

        // If we move the mitts out of Thieves Town, it should still show up as mandatory as it's a crystal dungeon
        var locations = world.Locations.Where(x => x.Region is ThievesTown).ToList();
        var mitts = world.FindLocation(LocationId.ThievesTownBigChest);
        var mittsOther = world.Locations.First(x => x is { Room: LightWorldNorthWest.BlindsHideoutRoom, ItemType: ItemType.ThreeBombs });
        SwapLocationItems(mitts, mittsOther);
        Assert.Equal(LocationUsefulness.Mandatory, hintService.GetUsefulness(locations, [world], null));

        // If we swap its reward with desert palace, it should now show up as useless
        SwapRegionRewards(world.ThievesTown, world.DesertPalace);
        Assert.Equal(LocationUsefulness.Useless, hintService.GetUsefulness(locations, [world], null));
        SwapRegionRewards(world.ThievesTown, world.DesertPalace);

        // If we set the ganon crystal requirement to 0, it should show up as useless
        world.Config.GameModeOptions.GanonCrystalCount = 0;
        world.Config.GameModeOptions.OpenPyramid = true;
        Assert.Equal(LocationUsefulness.Useless, hintService.GetUsefulness(locations, [world], null));

        // But if the player has to climb GT to access ganon, it should be mandatory
        world.Config.GameModeOptions.GanonCrystalCount = 0;
        world.Config.GameModeOptions.OpenPyramid = false;
        Assert.Equal(LocationUsefulness.Mandatory, hintService.GetUsefulness(locations, [world], null));

        // Unless GT also requires 0 crystals
        world.Config.GameModeOptions.GanonCrystalCount = 0;
        world.Config.GameModeOptions.GanonsTowerCrystalCount = 0;
        world.Config.GameModeOptions.OpenPyramid = false;
        Assert.Equal(LocationUsefulness.Useless, hintService.GetUsefulness(locations, [world], null));
    }

    [Fact]
    void VanillaGoalMetroidHints()
    {
        var world = GetVanillaWorld();
        var hintService = GetGameHintService();

        // Grapple is required to access Draygon to get space jump
        var locations = world.Locations.Where(x => x.ItemType is ItemType.Grapple).ToList();
        Assert.Equal(LocationUsefulness.Mandatory, hintService.GetUsefulness(locations, [world], null));

        // But if we only need two Metroid bosses, it should simply be nice to have
        world.Config.GameModeOptions.TourianBossCount = 2;
        world.Config.LogicConfig.MoatSpeedBooster = true;
        Assert.Equal(LocationUsefulness.NiceToHave, hintService.GetUsefulness(locations, [world], null));
    }

    [Fact]
    void VanillaGoalSwordAndSilversHints()
    {
        var world = GetVanillaWorld();
        world.Config.GameModeOptions.GanonCrystalCount = 0;
        world.Config.GameModeOptions.GanonsTowerCrystalCount = 0;
        world.Config.GameModeOptions.OpenPyramid = true;
        var hintService = GetGameHintService();

        // Move the bow and boots out of eastern palace for testing
        var bow = world.FindLocation(LocationId.EasternPalaceBigChest);
        var bowOtherItem = world.Locations.First(x => x is { Room: LightWorldNorthWest.BlindsHideoutRoom, ItemType: ItemType.ThreeBombs });
        SwapLocationItems(bow, bowOtherItem);
        var boots = world.FindLocation(LocationId.Sahasrahla);
        var bootsOtherItem = world.Locations.First(x => x is { Room: LightWorldNorthWest.BlindsHideoutRoom, ItemType: ItemType.ThreeBombs });
        SwapLocationItems(boots, bootsOtherItem);
        var locations = world.Locations.Where(x => x.Region is EasternPalace).ToList();
        Assert.Equal(LocationUsefulness.Useless, hintService.GetUsefulness(locations, [world], null));
        Assert.Equal(LocationUsefulness.Useless, hintService.GetUsefulness([boots], [world], null));

        // If we allow the player to only get one sword, it should register as unbeatable (useless locations will appear as mandatory)
        var swordLocations = world.Locations.Where(x =>
            x.Id != LocationId.LinksUncle && x.ItemType == ItemType.ProgressiveSword).ToList();
        foreach (var swordLocation in swordLocations)
        {
            swordLocation.Item = new Item(ItemType.TwentyRupees, world);
        }
        Assert.Equal(LocationUsefulness.Mandatory, hintService.GetUsefulness(swordLocations, [world], null));

        // But if we allow the player to skip Ganon, it should be beatable
        world.Config.GameModeOptions.LiftOffOnGoalCompletion = true;
        Assert.Equal(LocationUsefulness.Useless, hintService.GetUsefulness(swordLocations, [world], null));
        world.Config.GameModeOptions.LiftOffOnGoalCompletion = false;

        // Sahasrahla should be required if he has a sword
        var sahasrahla = world.FindLocation(LocationId.Sahasrahla);
        sahasrahla.Item = new Item(ItemType.ProgressiveSword, world);
        Assert.Equal(LocationUsefulness.Mandatory, hintService.GetUsefulness([sahasrahla], [world], null));

        // Which means the green pendant dungeon should be required as well
        Assert.Equal(LocationUsefulness.Mandatory, hintService.GetUsefulness(world.Locations.Where(x => x.Region is EasternPalace).ToList(), [world], null));

        // Pyramid fairy should also be mandatory for the silver arrows
        Assert.Equal(LocationUsefulness.Mandatory, hintService.GetUsefulness([world.FindLocation(LocationId.PyramidFairyRight)], [world], null));

        // Which means that the crystal dungeons should be required
        Assert.Equal(LocationUsefulness.Mandatory, hintService.GetUsefulness(world.Locations.Where(x => x.Region is MiseryMire).ToList(), [world], null));
        Assert.Equal(LocationUsefulness.Mandatory, hintService.GetUsefulness(world.Locations.Where(x => x.Region is IcePalace).ToList(), [world], null));
    }

    [Fact]
    void KeysanityHints()
    {
        var world = GetVanillaWorld(new GameModeOptions() { KeysanityMode = KeysanityMode.Both } );
        var hintService = GetGameHintService();

        world.FindLocation(LocationId.KakarikoTavern).Item = new Item(ItemType.ProgressiveSword, world);

        // Hhype cave should be useless and Thieve's Town should be mandatory
        var hypeCave = world.Locations.Where(x => x.Room is DarkWorldSouth.HypeCaveRoom).ToList();
        var thievesTown = world.Locations.Where(x => x.Region is ThievesTown).ToList();
        Assert.Equal(LocationUsefulness.Useless, hintService.GetUsefulness(hypeCave, [world], null));
        Assert.Equal(LocationUsefulness.Mandatory, hintService.GetUsefulness(thievesTown, [world], null));

        // Now if we move the TT big key into hype cave, hype cave should show up as having a key
        SwapLocationItems(hypeCave.First(), world.FindLocation(LocationId.ThievesTownBigKeyChest));
        Assert.Equal(LocationUsefulness.Key, hintService.GetUsefulness(hypeCave, [world], null));
        Assert.Equal(LocationUsefulness.Mandatory, hintService.GetUsefulness(thievesTown, [world], null));

        // Now let's move the gloves from TT into hype cave as well and make sure it shows up as mandatory
        SwapLocationItems(hypeCave.Skip(1).First(), world.FindLocation(LocationId.ThievesTownBigChest));
        Assert.Equal(LocationUsefulness.Mandatory, hintService.GetUsefulness(hypeCave, [world], null));
        Assert.Equal(LocationUsefulness.Key, hintService.GetUsefulness(thievesTown, [world], null));

        // Now let's make sure desert palace still shows as useless if there's nothing in there but its own keys
        SwapLocationItems(hypeCave.Skip(2).First(), world.FindLocation(LocationId.DesertPalaceBigChest));
        SwapLocationItems(hypeCave.Skip(3).First(), world.FindLocation(LocationId.DesertPalaceCompassChest));
        SwapLocationItems(hypeCave.Skip(4).First(), world.FindLocation(LocationId.DesertPalaceMapChest));
        var desertPalace = world.Locations.Where(x => x.Region is DesertPalace).ToList();
        Assert.Equal(LocationUsefulness.Useless, hintService.GetUsefulness(desertPalace, [world], null));

        // Make sure the crateria boss keycard location shows up as a key initially
        var crateriaBossKeycard = world.Locations.First(x => x.ItemType == ItemType.CardCrateriaBoss);
        Assert.Equal(LocationUsefulness.Key, hintService.GetUsefulness([crateriaBossKeycard], [world], null));

        // But if tourian boss door is disabled, then it should be marked as useless
        world.Config.GameModeOptions.SkipTourianBossDoor = true;
        Assert.Equal(LocationUsefulness.Useless, hintService.GetUsefulness([crateriaBossKeycard], [world], null));

        // Make sure the lower norfair boss keycard location shows up as a key initially
        var ridleyBossKeycard = world.Locations.First(x => x.ItemType == ItemType.CardLowerNorfairBoss);
        Assert.Equal(LocationUsefulness.Key, hintService.GetUsefulness([ridleyBossKeycard], [world], null));

        // But if tourian boss door is disabled, then it should be marked as useless
        world.Config.GameModeOptions.TourianBossCount = 3;
        Assert.Equal(LocationUsefulness.Useless, hintService.GetUsefulness([ridleyBossKeycard], [world], null));
    }

    [Fact]
    void MultiworldHints()
    {
        List<World> worlds =
        [
            GetVanillaWorld(playerId: 1),
            GetVanillaWorld(gameModeOptions: new GameModeOptions { KeysanityMode = KeysanityMode.Both }, playerId: 2)
        ];
        var hintService = GetGameHintService();

        // Make sure hype cave in both worlds starts out as mandatory
        var world1HypeCave = worlds[0].Locations.Where(x => x.Room is DarkWorldSouth.HypeCaveRoom).ToList();
        var world2HypeCave = worlds[1].Locations.Where(x => x.Room is DarkWorldSouth.HypeCaveRoom).ToList();
        Assert.Equal(LocationUsefulness.Useless, hintService.GetUsefulness(world1HypeCave, worlds, null));
        Assert.Equal(LocationUsefulness.Useless, hintService.GetUsefulness(world2HypeCave, worlds, null));

        // If we move player 2's bow into player 1's world, it should be marked as mandatory
        var tempLocation = worlds[1].FindLocation(LocationId.EasternPalaceBigChest);
        SwapLocationItems(world1HypeCave[0], tempLocation);
        Assert.Equal(LocationUsefulness.Mandatory, hintService.GetUsefulness(world1HypeCave, worlds, null));
        Assert.Equal(LocationUsefulness.Useless, hintService.GetUsefulness(world2HypeCave, worlds, null));
        SwapLocationItems(world1HypeCave[0], tempLocation);

        // If we move player 2's master sword into player 1's world, it should be marked as a mandatory
        tempLocation = worlds[1].FindLocation(LocationId.MasterSwordPedestal);
        SwapLocationItems(world1HypeCave[0], tempLocation);
        tempLocation = worlds[1].FindLocation(LocationId.Blacksmith);
        SwapLocationItems(world1HypeCave[1], tempLocation);
        tempLocation = worlds[1].FindLocation(LocationId.PyramidFairyLeft);
        SwapLocationItems(world1HypeCave[2], tempLocation);
        Assert.Equal(LocationUsefulness.Mandatory, hintService.GetUsefulness(world1HypeCave, worlds, null));
        Assert.Equal(LocationUsefulness.Useless, hintService.GetUsefulness(world2HypeCave, worlds, null));
        tempLocation = worlds[1].FindLocation(LocationId.MasterSwordPedestal);
        SwapLocationItems(world1HypeCave[0], tempLocation);
        tempLocation = worlds[1].FindLocation(LocationId.Blacksmith);
        SwapLocationItems(world1HypeCave[1], tempLocation);
        tempLocation = worlds[1].FindLocation(LocationId.PyramidFairyLeft);
        SwapLocationItems(world1HypeCave[2], tempLocation);

        // If player 2's silver arrows are in player 1's world, it should be marked as mandatory
        tempLocation = worlds[1].FindLocation(LocationId.PyramidFairyRight);
        SwapLocationItems(world1HypeCave[0], tempLocation);
        Assert.Equal(LocationUsefulness.Mandatory, hintService.GetUsefulness(world1HypeCave, worlds, null));
        Assert.Equal(LocationUsefulness.Useless, hintService.GetUsefulness(world2HypeCave, worlds, null));

        // Unless player 2 does not require silvers, then it's just nice to have
        worlds[1].Config.GameModeOptions.LiftOffOnGoalCompletion = true;
        Assert.Equal(LocationUsefulness.NiceToHave, hintService.GetUsefulness(world1HypeCave, worlds, null));
        Assert.Equal(LocationUsefulness.Useless, hintService.GetUsefulness(world2HypeCave, worlds, null));
        SwapLocationItems(world1HypeCave[0], tempLocation);

        // If player 1 has player 2's lower norfair boss key, it should be marked as a key item
        tempLocation = worlds[1].Locations.First(x => x.ItemType == ItemType.CardLowerNorfairBoss);
        SwapLocationItems(world1HypeCave[0], tempLocation);
        Assert.Equal(LocationUsefulness.Key, hintService.GetUsefulness(world1HypeCave, worlds, null));
        Assert.Equal(LocationUsefulness.Useless, hintService.GetUsefulness(world2HypeCave, worlds, null));

        // But if the player can skip Ridley, then it's useless
        worlds[1].Config.GameModeOptions.TourianBossCount = 3;
        Assert.Equal(LocationUsefulness.Useless, hintService.GetUsefulness(world1HypeCave, worlds, null));
        Assert.Equal(LocationUsefulness.Useless, hintService.GetUsefulness(world2HypeCave, worlds, null));
    }

    [Fact]
    void MetroidBossTokensHints()
    {
        var world = GetVanillaWorld(new GameModeOptions { ShuffleMetroidBossTokens = true } );
        var hintService = GetGameHintService();

        // Move the bow and boots out of eastern palace for testing
        var bow = world.FindLocation(LocationId.EasternPalaceBigChest);
        var bowOtherItem = world.Locations.First(x => x is { Room: LightWorldNorthWest.BlindsHideoutRoom, ItemType: ItemType.ThreeBombs });
        SwapLocationItems(bow, bowOtherItem);
        var boots = world.FindLocation(LocationId.Sahasrahla);
        var bootsOtherItem = world.Locations.First(x => x is { Room: LightWorldNorthWest.BlindsHideoutRoom, ItemType: ItemType.ThreeBombs });
        SwapLocationItems(boots, bootsOtherItem);
        Assert.Equal(LocationUsefulness.Useless, hintService.GetUsefulness(world.Locations.Where(x => x.Region is EasternPalace).ToList(), [world], null));

        // Move the kraid token into eastern palace and make sure it's necessary
        SwapRegionRewards(world.EasternPalace, world.KraidsLair);
        Assert.Equal(LocationUsefulness.Mandatory, hintService.GetUsefulness(world.Locations.Where(x => x.Region is EasternPalace).ToList(), [world], null));
    }

    [Fact]
    void SpazerHuntHints()
    {
        var world = GetVanillaWorld(new GameModeOptions() { SelectedGameModeType = GameModeType.SpazerHunt });
        var hintService = GetGameHintService();

        Assert.Equal(LocationUsefulness.Goal, hintService.GetUsefulness([world.FindLocation(LocationId.RedBrinstarSpazer)], [world], null));

        // Move the bow and boots out of eastern palace for testing
        var bow = world.FindLocation(LocationId.EasternPalaceBigChest);
        var bowOtherItem = world.Locations.First(x => x is { Room: LightWorldNorthWest.BlindsHideoutRoom, ItemType: ItemType.ThreeBombs });
        SwapLocationItems(bow, bowOtherItem);
        var boots = world.FindLocation(LocationId.Sahasrahla);
        var bootsOtherItem = world.Locations.First(x => x is { Room: LightWorldNorthWest.BlindsHideoutRoom, ItemType: ItemType.ThreeBombs });
        SwapLocationItems(boots, bootsOtherItem);
        Assert.Equal(LocationUsefulness.Useless, hintService.GetUsefulness(world.Locations.Where(x => x.Region is EasternPalace).ToList(), [world], null));

        // If Sahasrahla has a spazer, eastern palace should be marked as on the way to the goal
        SwapLocationItems(boots, world.FindLocation(LocationId.RedBrinstarSpazer));
        Assert.Equal(LocationUsefulness.Goal, hintService.GetUsefulness(world.Locations.Where(x => x.Region is EasternPalace).ToList(), [world], null));
    }

    [Fact]
    void SpazerHuntMultiworldHints()
    {
        List<World> worlds =
        [
            GetVanillaWorld(playerId: 1),
            GetVanillaWorld(gameModeOptions: new GameModeOptions { SelectedGameModeType = GameModeType.SpazerHunt}, playerId: 2)
        ];
        var hintService = GetGameHintService();

        // World 1 should say a spazer is nice to have, world 2 should say it's a goal
        Assert.Equal(LocationUsefulness.NiceToHave, hintService.GetUsefulness([worlds[0].FindLocation(LocationId.RedBrinstarSpazer)], worlds, null));
        Assert.Equal(LocationUsefulness.Goal, hintService.GetUsefulness([worlds[1].FindLocation(LocationId.RedBrinstarSpazer)], worlds, null));

        // If we move player 2's spazers to world 1, it should show it as a goal, but mandatory if other high priority items are there
        var spazer = worlds[1].FindLocation(LocationId.RedBrinstarSpazer);
        var spazerOtherLocation = worlds[0].Locations.First(x => x is { Room: LightWorldNorthWest.BlindsHideoutRoom, ItemType: ItemType.ThreeBombs });
        SwapLocationItems(spazer, spazerOtherLocation);
        Assert.Equal(LocationUsefulness.Goal, hintService.GetUsefulness([spazerOtherLocation], worlds, null));
        Assert.Equal(LocationUsefulness.Mandatory, hintService.GetUsefulness(worlds[0].Locations.Where(x => x.Room is LightWorldNorthWest.BlindsHideoutRoom).ToList(), worlds, null));
    }

    [Fact]
    void AllDungeonsHints()
    {
        var world = GetVanillaWorld(new GameModeOptions() { SelectedGameModeType = GameModeType.AllDungeons });
        var hintService = GetGameHintService();

        // Move the bow and boots out of eastern palace for testing
        var bow = world.FindLocation(LocationId.EasternPalaceBigChest);
        var bowOtherItem = world.Locations.First(x => x is { Room: LightWorldNorthWest.BlindsHideoutRoom, ItemType: ItemType.ThreeBombs });
        SwapLocationItems(bow, bowOtherItem);
        var boots = world.FindLocation(LocationId.Sahasrahla);
        var bootsOtherItem = world.Locations.First(x => x is { Room: LightWorldNorthWest.BlindsHideoutRoom, ItemType: ItemType.ThreeBombs });
        SwapLocationItems(boots, bootsOtherItem);

        // Eastern palace should be mandatory for the all dungeons, but useless for vanilla
        Assert.Equal(LocationUsefulness.Mandatory, hintService.GetUsefulness(world.Locations.Where(x => x.Region is EasternPalace).ToList(), [world], null));
        world.Config.GameModeOptions.SelectedGameModeType = GameModeType.Vanilla;
        Assert.Equal(LocationUsefulness.Useless, hintService.GetUsefulness(world.Locations.Where(x => x.Region is EasternPalace).ToList(), [world], null));
    }

    #region Private methods
    private void SwapLocationItems(Location one, Location two)
    {
        (one.Item, two.Item) = (two.Item, one.Item);
    }

    private void SwapRegionRewards(IHasReward one, IHasReward two)
    {
        var temp = one.Reward;
        one.SetReward(two.Reward);
        two.SetReward(temp);
    }

    private World GetVanillaWorld(GameModeOptions? gameModeOptions = null, LogicConfig? logicConfig = null, int playerId = 1)
    {
        gameModeOptions ??= new GameModeOptions();
        logicConfig ??= new LogicConfig();
        var isKeysanity = gameModeOptions.KeysanityMode == KeysanityMode.Both;
        var config = new Config() { GameModeOptions = gameModeOptions, LogicConfig = logicConfig };
        var world = new World(config, playerId.ToString(), playerId, Guid.NewGuid().ToString("N"), isLocalWorld: playerId == 1);
        var metroidKeycards = world.ItemPools.MetroidKeysanityItems.ToList();
        foreach (var location in world.Locations)
        {
            var itemType = location.VanillaItem;
            if (location.Id == LocationId.BlindsHideoutLeft)
            {
                itemType = ItemType.Bombs;
            }
            else if (location.Id == LocationId.BlindsHideoutRight)
            {
                itemType = ItemType.Super;
            }
            else if (itemType == ItemType.Nothing && location.Region is GanonsTower)
            {
                itemType = ItemType.KeyGT;
            }
            else if (itemType == ItemType.Nothing)
            {
                itemType = ItemType.ThreeBombs;
            }

            if (isKeysanity)
            {
                if (metroidKeycards.Count > 0 && (itemType.IsInCategory(ItemCategory.Compass) || itemType.IsInCategory(ItemCategory.Map)))
                {
                    itemType = metroidKeycards.First().Type;
                    metroidKeycards.RemoveAt(0);
                }
                else if (location.Id == LocationId.BlindsHideoutTop)
                {
                    itemType = ItemType.KeyPD;
                }
            }

            location.Item = new Item(itemType, world);
        }

        (world.KraidsLair as IHasReward).SetReward(new Reward(RewardType.KraidToken, world, null));
        (world.WreckedShip as IHasReward).SetReward(new Reward(RewardType.PhantoonToken, world, null));
        (world.InnerMaridia as IHasReward).SetReward(new Reward(RewardType.DraygonToken, world, null));
        (world.LowerNorfairEast as IHasReward).SetReward(new Reward(RewardType.RidleyToken, world, null));

        return world;
    }

    private static IGameHintService GetGameHintService()
    {
        var serviceProvider = new ServiceCollection()
            .AddLogging(_ => { })
            .AddSingleton<OptionsFactory>()
            .AddSingleton<ConfigProvider>()
            .AddSingleton<RomPatchFactory>()
            .AddSingleton<IMetadataService, MetadataService>()
            .AddSingleton<IGameHintService, GameHintService>()
            .AddSingleton<IPatcherService, PatcherService>()
            .AddSingleton<TrackerOptionsAccessor>()
            .AddSingleton<TrackerSpriteService>()
            .AddTransient<GameModeFactory>()
            .AddTransient<PlaythroughService>()
            .AddGameModes<GameModeFactory>()
            .AddConfigs()
            .BuildServiceProvider();

        return serviceProvider.GetRequiredService<IGameHintService>();
    }
    #endregion
}

internal static class ServiceCollectionExtensions
{
    private static readonly Lock s_lockObject = new();
    private static bool s_addedGameModes;

    internal static IServiceCollection AddGameModes<TAssembly>(this IServiceCollection services)
    {
        lock (s_lockObject)
        {
            var moduleTypes = typeof(TAssembly).Assembly.GetTypes()
                .Where(x => x.IsSubclassOf(typeof(GameModeBase)));

            foreach (var moduleType in moduleTypes)
            {
                services.TryAddScoped(moduleType);

                if (!s_addedGameModes)
                {
                    GameModeFactory.AddGameModeClass(moduleType);
                }
            }

            s_addedGameModes = true;

            services.AddSingleton<GameModeFactory>();

            return services;
        }
    }
}
