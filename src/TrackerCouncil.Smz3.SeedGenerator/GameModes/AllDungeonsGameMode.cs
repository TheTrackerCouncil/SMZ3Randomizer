using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Data.GeneratedData;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Data.ParsedRom;
using TrackerCouncil.Smz3.Data.Services;
using TrackerCouncil.Smz3.Data.Tracking;
using TrackerCouncil.Smz3.Data.ViewModels;
using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.SeedGenerator.FileData.IpsPatches;
using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.Shared.Models;

namespace TrackerCouncil.Smz3.SeedGenerator.GameModes;

[GameModeType(GameModeType.AllDungeons)]
[Description("Complete all dungeons including Castle Tower and Ganon's Tower and defeat all 4 main Metroid bosses to be able to defeat Ganon. Defeat all 4 main bosses in Metroid to be able to enter Tourian and defeat Mother Brain.")]
public class AllDungeonsGameMode(IMetadataService metadata) : GameModeBase
{
    private TrackerBase _tracker = null!;

    public override void InitializeTracker(TrackerBase tracker)
    {
        _tracker = tracker;
        _tracker.BossTracker.BossUpdated += BossTrackerOnBossUpdated;
        _tracker.RewardTracker.RewardsChanged += RewardTrackerOnRewardsChanged;
    }

    private void RewardTrackerOnRewardsChanged(object? sender, EventArgs e)
    {
        if (IsComplete(_tracker.World))
        {
            _tracker.GameModeService.MarkGameModeAsComplete();
        }

        _tracker.GameModeService.NotifyOfGoalStateChange();
    }

    private void BossTrackerOnBossUpdated(object? sender, BossTrackedEventArgs e)
    {
        if (IsComplete(_tracker.World))
        {
            _tracker.GameModeService.MarkGameModeAsComplete();
        }

        _tracker.GameModeService.NotifyOfGoalStateChange();
    }

    public override bool IsComplete(World world)
    {
        if (world.State == null)
        {
            return false;
        }

        var progression = _tracker.PlayerProgressionService.GetProgression(false);

        return progression is { AllPendants: true, AllCrystals: true } &&
               progression.MetroidBossCount >= 4 &&
               world.AllBosses.First(x => x.Type == BossType.Agahnim).Defeated &&
               (_tracker.AutoTracker?.OpenPyramid == true || world.AllBosses.First(x => x.Type == BossType.Ganon).Defeated || world.Config.GameModeOptions.OpenPyramid);
    }

    public override void UpdateWorld(World world, Random rng, GameModeOptions gameModeOptions)
    {
        gameModeOptions.GanonCrystalCount = 7;
        gameModeOptions.TourianBossCount = 4;
        gameModeOptions.OpenPyramid = false;
    }

    public override void UpdateInitialTrackerState(GameModeOptions gameModeOptions, TrackerState trackerState,
        ParsedRomDetails? parsedRomDetails)
    {
        trackerState.MarkedGanonCrystalCount = 7;
        trackerState.MarkedTourianBossCount = 4;
    }

    public override GameModeInGameText GetInGameText(World world)
    {
        return new GameModeInGameText
        {
            GanonIntro = metadata.GameModes.AllDungeonsGanonIntro,
            GanonIntroGoalsNotMet =  metadata.GameModes.AllDungeonsGanonIntroGoalsNotMet,
            GanonGoalSign = metadata.GameModes.AllDungeonsGanonGoalSign,
        };
    }

    public override string? GetGameStartText(World world)
    {
        return metadata.GameModes.AllDungeonsGameStarted?.ToString();
    }

    public override bool IsKnowinglyComplete(World world)
    {
        return IsComplete(world);
    }

    public override List<GoalUiDetails> GetGoalUiDetails(World world, Progression progression)
    {
        var dungeons = progression.CrystalCount + progression.PendantCount;

        if (world.AllBosses.First(x => x.Type == BossType.Agahnim).Defeated)
        {
            dungeons++;
        }

        if (_tracker.AutoTracker?.OpenPyramid == true || world.AllBosses.First(x => x.Type == BossType.Ganon).Defeated || world.Config.GameModeOptions.OpenPyramid)
        {
            dungeons++;
        }

        var bosses = progression.MetroidBossCount;
        return
        [
            new GoalUiDetails { IconCategory = "Items", Icon = "map_full.png", Text = $"{dungeons}/12" },
            new GoalUiDetails { IconCategory = "Dungeons", Icon = "metroid boss token.png", Text = $"{bosses}/4" }
        ];
    }

    public override string GetSpoilerText(GameModeOptions gameModeOptions)
    {
        return "";
    }

    public override Stream GetLiftOffOnGoalCompletionIpsPatch()
    {
        return IpsPatch.LiftOffOnGoalCompletionAllDungeons();
    }
}
