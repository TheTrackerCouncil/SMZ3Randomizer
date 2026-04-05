using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Data.GeneratedData;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Data.Services;
using TrackerCouncil.Smz3.Data.Tracking;
using TrackerCouncil.Smz3.Data.ViewModels;
using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.Shared;
using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.Shared.Models;

namespace TrackerCouncil.Smz3.SeedGenerator.AltGameModes;

[GameModeType(GameModeType.SpazerHunt)]
[Description("You thought they were worthless, but Samus needs to collect all of the spazers and use them to save the day. After all, who needs a Triforce when you have lots of firepower? Requires auto tracking.")]
public class SpazerHuntAltGameMode(IMetadataService metadata) : AltGameModeBase
{
    private TrackerBase _tracker = null!;

    public override bool IsComplete(World world)
    {
        if (world.State == null)
        {
            return false;
        }

        var allSpazerLocations = world.State.LocationStates
            .Where(x => x.ItemWorldId == world.Id && x.Item == ItemType.Spazer).ToList();
        var spazerCount = allSpazerLocations.Count(x => x.Cleared);
        return spazerCount >= world.Config.GameModeOptions.SpazersRequired;
    }

    public override void UpdateInitialTrackerState(GameModeOptions gameModeOptions, TrackerState trackerState)
    {
        if (gameModeOptions.RandomizeGoalCounts)
        {
            trackerState.AltGameModeState = "";
        }
        else
        {
            trackerState.AltGameModeState = $"{gameModeOptions.SpazersRequired}";
        }
    }

    public override void UpdateWorld(World world, int seed, GameModeOptions gameModeOptions)
    {
        var rng = new Random(seed);
        rng.Sanitize();

        var swaps = new List<SwapItemPoolRequest>();

        var spazersInPool = gameModeOptions.SpazersInPool;
        if (gameModeOptions.RandomizeGoalCounts)
        {
            spazersInPool = rng.Next(gameModeOptions.MinSpazersInPool, gameModeOptions.MaxSpazersInPool + 1);
            gameModeOptions.SpazersInPool = spazersInPool;

            gameModeOptions.SpazersRequired =
                rng.Next(gameModeOptions.MinSpazersRequired, Math.Min(spazersInPool, gameModeOptions.MaxSpazersRequired));
        }

        var spazersToSpawn = spazersInPool;

        // First try to remove really useless items people won't miss for spazers
        List<ItemType> junkItemTypes = [ItemType.TwentyRupees, ItemType.TwentyRupees2, ItemType.Arrow, ItemType.TenArrows, ItemType.FiveRupees, ItemType.OneRupee];
        foreach (var item in world.ItemPools.Junk.Where(x => junkItemTypes.Contains(x.Type)))
        {
            swaps.Add(new SwapItemPoolRequest
            {
                FromItemType = item.Type,
                ToItemType = ItemType.Spazer,
                IsNiceToHave = true
            });
            spazersToSpawn--;

            if (spazersToSpawn <= 0)
            {
                break;
            }
        }

        // Next swap out heart pieces for heart containers to add spazers
        if (spazersToSpawn >= 3)
        {
            for (var i = 0; i < world.ItemPools.Junk.Count(x => x.Type == ItemType.HeartPiece); i += 4)
            {
                swaps.Add(new SwapItemPoolRequest
                {
                    FromItemType = ItemType.HeartPiece,
                    ToItemType = ItemType.HeartContainer,
                });

                swaps.Add(new SwapItemPoolRequest
                {
                    FromItemType = ItemType.HeartPiece,
                    ToItemType = ItemType.Spazer,
                    IsNiceToHave = true
                });

                swaps.Add(new SwapItemPoolRequest
                {
                    FromItemType = ItemType.HeartPiece,
                    ToItemType = ItemType.Spazer,
                    IsNiceToHave = true
                });

                swaps.Add(new SwapItemPoolRequest
                {
                    FromItemType = ItemType.HeartPiece,
                    ToItemType = ItemType.Spazer,
                    IsNiceToHave = true
                });

                spazersToSpawn -= 3;
                if (spazersToSpawn < 3)
                {
                    break;
                }
            }
        }

        // Next swap out larger rupee and three bombs drops for spazers
        if (spazersToSpawn > 0)
        {
            junkItemTypes = [ItemType.FiftyRupees, ItemType.OneHundredRupees, ItemType.ThreeBombs];
            foreach (var item in world.ItemPools.Junk.Where(x => junkItemTypes.Contains(x.Type)))
            {
                swaps.Add(new SwapItemPoolRequest
                {
                    FromItemType = item.Type,
                    ToItemType = ItemType.Spazer,
                    IsNiceToHave = true
                });
                spazersToSpawn--;

                if (spazersToSpawn <= 0)
                {
                    break;
                }
            }
        }

        // Lastly fallback to bomb and arrow upgrades and some missiles
        junkItemTypes =
        [
            ItemType.BombUpgrade5, ItemType.ArrowUpgrade5, ItemType.Missile, ItemType.Missile, ItemType.Missile, ItemType.Missile,
            ItemType.BombUpgrade5, ItemType.ArrowUpgrade5, ItemType.Missile, ItemType.Missile, ItemType.Missile, ItemType.Missile,
            ItemType.BombUpgrade5, ItemType.ArrowUpgrade5, ItemType.BombUpgrade5, ItemType.ArrowUpgrade5,
        ];
        for (var i = 0; i < spazersToSpawn; i++)
        {
            swaps.Add(new SwapItemPoolRequest
            {
                FromItemType = junkItemTypes[0],
                ToItemType = ItemType.Spazer,
                IsNiceToHave = true
            });
            junkItemTypes.RemoveAt(0);
        }

        world.ItemPools.SwapItems(swaps);
    }

    public override void InitializeTracker(TrackerBase tracker)
    {
        _tracker = tracker;

        _tracker.LocationTracker.LocationCleared += LocationTrackerOnLocationCleared;
        _tracker.ItemTracker.ItemTracked += ItemTrackerOnItemTracked;
    }

    public override AltGameModeInGameText? GetInGameText(World world)
    {
        var spazersRequired = world.Config.GameModeOptions.SpazersRequired;
        var spazersInPool = world.Config.GameModeOptions.SpazersInPool;
        return new AltGameModeInGameText
        {
            GanonIntro = metadata.AltGameModes.SpazerHuntGanonIntro?.Format(spazersRequired, spazersInPool),
            GanonIntroGoalsNotMet =
                metadata.AltGameModes.SpazerHuntGanonIntroGoalsNotMet?.Format(spazersRequired, spazersInPool),
            GanonGoalSign = metadata.AltGameModes.SpazerHuntGanonGoalSign?.Format(spazersRequired, spazersInPool),
        };
    }

    public override string? GetGameStartText(World world)
    {
        return metadata.AltGameModes.SpazerHuntGameStarted?.ToString();
    }

    public override bool IsKnowinglyComplete(World world)
    {
        return world.State?.AltGameModeState == $"{world.Config.GameModeOptions.SpazersRequired}" && IsComplete(world);
    }

    public override bool OnViewingPyramidText(World world)
    {
        if (world.State != null && world.State.AltGameModeState != $"{world.Config.GameModeOptions.SpazersRequired}")
        {
            world.State.AltGameModeState = $"{world.Config.GameModeOptions.SpazersRequired}";
            return true;
        }

        return false;
    }

    public override List<Location>? GetGameModeLocations(World world)
    {
        return world.Locations.Where(x => x.ItemType is ItemType.Spazer).ToList();
    }

    public override List<GoalUiDetails>? GetGoalUiDetails(World world)
    {
        if (world.State == null)
        {
            return null;
        }

        var allSpazerLocations = world.State.LocationStates
            .Where(x => x.ItemWorldId == world.Id && x.Item == ItemType.Spazer).ToList();
        var spazerCount = allSpazerLocations.Count(x => x.Cleared);
        var spazersNeeded = string.IsNullOrEmpty(world.State?.AltGameModeState) ? "?" : world.State.AltGameModeState;
        return
        [
            new GoalUiDetails { IconCategory = "Items", Icon = "spazer.png", Text = $"{spazerCount}/{spazersNeeded}" }
        ];
    }

    private void LocationTrackerOnLocationCleared(object? sender, LocationClearedEventArgs e)
    {
        if (!e.AutoTracked || e.Location.ItemType != ItemType.Spazer)
        {
            return;
        }

        if (IsComplete(_tracker.World))
        {
            _tracker.AltGameModeService.MarkAltGameModeAsComplete();
        }

        _tracker.AltGameModeService.NotifyOfGoalStateChange();
    }

    private void ItemTrackerOnItemTracked(object? sender, ItemTrackedEventArgs e)
    {
        if (!e.AutoTracked || e.Item?.Type != ItemType.Spazer)
        {
            return;
        }

        if (IsComplete(_tracker.World))
        {
            _tracker.AltGameModeService.MarkAltGameModeAsComplete();
        }

        _tracker.AltGameModeService.NotifyOfGoalStateChange();
    }
}
