using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Data.GeneratedData;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Data.ParsedRom;
using TrackerCouncil.Smz3.Data.Services;
using TrackerCouncil.Smz3.Data.ViewModels;
using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.SeedGenerator.FileData.IpsPatches;
using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.Shared.Models;

namespace TrackerCouncil.Smz3.SeedGenerator.GameModes;

[GameModeType(GameModeType.Vanilla)]
[Description("Complete Zelda dungeons to obtain crystals to be able to defeat Ganon. Defeat bosses in Super Metroid to be able to enter Tourian and defeat Mother Brain.")]
public class VanillaGameMode(IMetadataService metadata) : GameModeBase
{
    private TrackerBase _tracker = null!;

    public override void InitializeTracker(TrackerBase tracker)
    {
        _tracker = tracker;
        _tracker.RewardTracker.RewardsChanged += RewardTrackerOnRewardsChanged;
    }

    public override bool IsComplete(World world)
    {
        if (world.State == null)
        {
            return false;
        }

        var progression = _tracker.PlayerProgressionService.GetProgression(false);

        return progression.CrystalCount >= world.Config.GameModeOptions.GanonCrystalCount &&
               progression.MetroidBossCount >= world.Config.GameModeOptions.TourianBossCount;
    }

    public override void UpdateWorld(World world, Random rng, GameModeOptions gameModeOptions)
    {
        if (gameModeOptions.RandomizeNumericAmounts)
        {
            gameModeOptions.GanonCrystalCount = rng.Next(gameModeOptions.MinGanonCrystalCount, gameModeOptions.MaxGanonCrystalCount + 1);
            gameModeOptions.TourianBossCount = rng.Next(gameModeOptions.MinTourianBossCount, gameModeOptions.MaxTourianBossCount + 1);
        }
    }

    public override GameModeInGameText GetInGameText(World world)
    {
        var ganonCrystalCount = world.Config.GameModeOptions.GanonCrystalCount;
        return new GameModeInGameText
        {
            GanonIntro = metadata.GameLines.GanonIntro,
            GanonIntroGoalsNotMet =  metadata.GameLines.GanonIntroGoalsNotMet,
            GanonGoalSign = metadata.GameLines.GanonGoalSign?.Format(ganonCrystalCount == 1 ? "1 crystal" : $"{ganonCrystalCount} crystals"),
        };
    }

    public override void UpdateInitialTrackerState(GameModeOptions gameModeOptions, TrackerState trackerState, ParsedRomDetails? parsedRomDetails)
    {
        int? markedGanonCrystalCount = null;
        int? markedTourianBossCount = null;

        if (parsedRomDetails == null)
        {
            if (gameModeOptions.RandomizeNumericAmounts)
            {
                markedGanonCrystalCount =
                    gameModeOptions.MinGanonCrystalCount == gameModeOptions.MaxGanonCrystalCount
                        ? gameModeOptions.MinGanonCrystalCount
                        : null;
                markedTourianBossCount =
                    gameModeOptions.MinTourianBossCount == gameModeOptions.MaxTourianBossCount
                        ? gameModeOptions.MinTourianBossCount
                        : null;
            }
            else
            {
                markedGanonCrystalCount = gameModeOptions.GanonCrystalCount;
                markedTourianBossCount = gameModeOptions.TourianBossCount;
            }
        }

        trackerState.MarkedGanonCrystalCount = markedGanonCrystalCount;
        trackerState.MarkedTourianBossCount = markedTourianBossCount;
    }

    public override List<GoalUiDetails> GetGoalUiDetails(World world, Progression progression)
    {
        var crystals = progression.CrystalCount;
        var bosses = progression.MetroidBossCount;
        var crystalsRequired = world.State?.MarkedGanonCrystalCount != null
            ? $"{world.State?.MarkedGanonCrystalCount}"
            : "?";
        var bossesRequired = world.State?.MarkedTourianBossCount != null
            ? $"{world.State?.MarkedTourianBossCount}"
            : "?";
        return
        [
            new GoalUiDetails { IconCategory = "Dungeons", Icon = "crystal.png", Text = $"{crystals}/{crystalsRequired}" },
            new GoalUiDetails { IconCategory = "Dungeons", Icon = "metroid boss token.png", Text = $"{bosses}/{bossesRequired}" }
        ];
    }

    public override string GetSpoilerText(GameModeOptions gameModeOptions)
    {
        return $"GanonCrystalCount = {gameModeOptions.GanonCrystalCount}, TourianBossCount = {gameModeOptions.TourianBossCount}";
    }

    public override Stream GetLiftOffOnGoalCompletionIpsPatch()
    {
        return IpsPatch.LiftOffOnGoalCompletionPrimary();
    }

    public override bool IsKnowinglyComplete(World world)
    {
        return world.State?.MarkedGanonCrystalCount == world.Config.GameModeOptions.GanonCrystalCount &&
               world.State?.MarkedTourianBossCount == world.Config.GameModeOptions.TourianBossCount &&
               IsComplete(world);
    }

    public override bool OnViewingPyramidText(World world)
    {
        if (world.State != null && world.State.MarkedGanonCrystalCount != world.Config.GameModeOptions.GanonCrystalCount)
        {
            _tracker.GameStateTracker.UpdateGanonRequirement(world.State?.GanonCrystalCount ?? world.Config.GameModeOptions.GanonCrystalCount, true);
            return true;
        }

        return false;
    }

    private void RewardTrackerOnRewardsChanged(object? sender, EventArgs e)
    {
        if (IsComplete(_tracker.World))
        {
            _tracker.GameModeService.MarkGameModeAsComplete();
        }

        _tracker.GameModeService.NotifyOfGoalStateChange();
    }
}
