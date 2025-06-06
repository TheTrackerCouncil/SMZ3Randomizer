﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Data.Tracking;
using TrackerCouncil.Smz3.Data.WorldData.Regions;
using TrackerCouncil.Smz3.Multiplayer.Client;
using TrackerCouncil.Smz3.Multiplayer.Client.EventHandlers;
using TrackerCouncil.Smz3.Shared;
using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.Tracking.Services;

namespace TrackerCouncil.Smz3.Tracking.VoiceCommands;

public class MultiplayerModule : TrackerModule
{
    private readonly MultiplayerGameService _multiplayerGameService;

    /// <summary>
    /// Initializes a new instance of the <see cref="MultiplayerModule"/>
    /// class.
    /// </summary>
    /// <param name="tracker">The tracker instance.</param>
    /// <param name="playerProgressionService">Service to get item information</param>
    /// <param name="worldQueryService">Service to get world information</param>
    /// <param name="logger">Used to write logging information.</param>
    /// <param name="multiplayerGameService">The multiplayer game service</param>
    /// <param name="autoTrackerBase"></param>
    public MultiplayerModule(TrackerBase tracker, IPlayerProgressionService playerProgressionService, IWorldQueryService worldQueryService,
        ILogger<MultiplayerModule> logger, MultiplayerGameService multiplayerGameService, AutoTrackerBase autoTrackerBase)
        : base(tracker, playerProgressionService, worldQueryService, logger)
    {
        _multiplayerGameService = multiplayerGameService;

        if (TrackerBase.Rom is { MultiplayerGameDetails: null }) return;

        TrackerBase.LocationTracker.LocationCleared += TrackerOnLocationCleared;
        TrackerBase.BossTracker.BossUpdated += TrackerOnBossUpdated;
        TrackerBase.ItemTracker.ItemTracked += TrackerOnItemTracked;
        TrackerBase.GameStateTracker.BeatGame += TrackerOnBeatGame;
        TrackerBase.GameStateTracker.PlayerDied += TrackerOnPlayerDied;
        autoTrackerBase.AutoTrackerConnected += AutoTrackerOnAutoTrackerConnected;

        _multiplayerGameService.PlayerTrackedLocation += PlayerTrackedLocation;
        _multiplayerGameService.PlayerTrackedItem += PlayerTrackedItem;
        _multiplayerGameService.PlayerTrackedBoss += PlayerTrackedBoss;
        _multiplayerGameService.PlayerTrackedDeath += PlayerTrackedDeath;
        _multiplayerGameService.PlayerSyncReceived += PlayerSyncReceived;
        _multiplayerGameService.PlayerEndedGame += PlayerEndedGame;

        _multiplayerGameService.SetTrackerState(worldQueryService.World.State!);
        _multiplayerGameService.OnTrackingStarted();

        Logger.LogInformation("Multiplayer module initialized");
    }

    private void AutoTrackerOnAutoTrackerConnected(object? sender, EventArgs e)
    {
        Logger.LogInformation("AutoTracker Connected");

        // Unfortunately sending items immediately after auto tracker has connected seems to cause the items to get
        // lost, so wait 5 seconds before trying to send the items
        Task.Run(async () =>
        {
            try
            {
                do
                {
                    await Task.Delay(TimeSpan.FromSeconds(5));
                } while (TrackerBase.AutoTracker?.HasValidState != true || TrackerBase.AutoTracker!.CurrentGame == Game.Neither);

                Logger.LogInformation("Requesting player sync");
                await _multiplayerGameService.OnAutoTrackingConnected();
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error starting multiworld");
            }
        });
    }

    private void PlayerSyncReceived(PlayerSyncReceivedEventHandlerArgs args)
    {
        // Ignore the sync if auto tracker is not connected as we don't want to lose out on items
        if (TrackerBase.AutoTracker?.HasValidState != true) return;
        if (args.PlayerId == null || args.ItemsToGive == null || args.ItemsToGive.Count == 0 || args.IsLocalPlayer) return;
        var items = args.ItemsToGive.Select(x => WorldQueryService.FirstOrDefault(x)).NonNull().ToList();

        Logger.LogInformation("Giving player {Count} items", items.Count());
        _ = TrackerBase.GameService!.TryGiveItemsAsync(items, args.PlayerId.Value);

        if ((args.DidForfeit || args.DidComplete) && WorldQueryService.Worlds.Any(x => x.Id == args.PlayerId))
        {
            WorldQueryService.Worlds.First(x => x.Id == args.PlayerId).HasCompleted = true;
        }

        foreach (var locationState in args.UpdatedLocationStates)
        {
            locationState.Cleared = true;
            locationState.Autotracked = true;
        }

        foreach (var itemState in args.UpdatedItemStates)
        {
            itemState.State.TrackingState = itemState.TrackingValue;
        }

        foreach (var bossState in args.UpdatedBossStates)
        {
            bossState.Defeated = true;
            bossState.AutoTracked = true;
        }
    }

    private void PlayerEndedGame(PlayerEndedGameEventHandlerArgs args)
    {
        // Comment on player forfeiting or completing
        if (args.IsLocalPlayer || _multiplayerGameService.LocalPlayer == null ||
            _multiplayerGameService.LocalPlayer.HasCompleted ||
            _multiplayerGameService.LocalPlayer.HasForfeited) return;
        if (args.DidComplete)
        {
            TrackerBase.Say(x => x.Multiplayer.OtherPlayerBeatGame, args: [args.PhoneticName]);
        }
        else if (args.DidForfeit)
        {
            TrackerBase.Say(x => x.Multiplayer.OtherPlayerForfeitGame, args: [args.PhoneticName]);
        }
    }

    private void PlayerTrackedItem(PlayerTrackedItemEventHandlerArgs args)
    {
        args.ItemState.TrackingState = args.TrackingValue;
        if (args.ItemState.Type == null || args.IsLocalPlayer) return;
        var item = WorldQueryService.FirstOrDefault(args.ItemState.Type.Value);
        if (item == null || item.State.TrackingState >= args.TrackingValue || !item.Progression)
        {
            return;
        }

        TrackerBase.Say(x => x.Multiplayer.OtherPlayerTrackedItem,
            args: [args.PhoneticName, item.Metadata.Name, item.Metadata.NameWithArticle]);
    }

    private void PlayerTrackedLocation(PlayerTrackedLocationEventHandlerArgs args)
    {
        // Ignore the sync if auto tracker is not connected as we don't want to lose out on items
        if (TrackerBase.AutoTracker?.HasValidState != true) return;
        if (args.ItemToGive == ItemType.Nothing) return;
        var item = WorldQueryService.FirstOrDefault(args.ItemToGive);
        if (item == null)
            throw new InvalidOperationException($"Player retrieved invalid item {args.ItemToGive}");
        _ = TrackerBase.GameService!.TryGiveItemAsync(item, args.PlayerId);
        if (item.Type.IsPossibleProgression(item.World.Config.ZeldaKeysanity, item.World.Config.MetroidKeysanity, item.IsLocalPlayerItem))
        {
            TrackerBase.Say(x => x.Multiplayer.ReceivedUsefulItemFromOtherPlayer,
                args: [args.PhoneticName, item.Metadata.Name, item.Metadata.NameWithArticle]);
        }
        else if (item.Type.IsInCategory(ItemCategory.Junk))
        {
            TrackerBase.Say(x => x.Multiplayer.ReceivedJunkItemFromOtherPlayer,
                args: [args.PhoneticName, item.Metadata.Name, item.Metadata.NameWithArticle]);
        }
        args.LocationState.Cleared = true;
        args.LocationState.Autotracked = true;
    }

    private void PlayerTrackedBoss(PlayerTrackedBossEventHandlerArgs args)
    {
        args.BossState.Defeated = true;
        args.BossState.AutoTracked = true;

        if (args.BossState.Type == BossType.None)
        {
            return;
        }

        var boss = WorldQueryService.Worlds.FirstOrDefault(x => x.Id == args.PlayerId)?.Bosses
            .FirstOrDefault(x => x.Type == args.BossState.Type);
        if (boss?.Region == null)
        {
            return;
        }

        // Check if the region also has treasure, thus it's a Zelda dungeon
        if (boss.Region is IHasTreasure treasureRegion)
        {
            if (boss.Region is IHasReward rewardRegion && rewardRegion.RewardType.IsInAnyCategory(RewardCategory.Pendant, RewardCategory.Crystal))
            {
                TrackerBase.Say(x => x.Multiplayer.OtherPlayerClearedDungeonWithReward,
                    args: [
                        args.PhoneticName,
                        rewardRegion.Metadata.Name, boss.Region.RandomBossName, rewardRegion.RewardMetadata.Name,
                        rewardRegion.RewardMetadata.NameWithArticle
                    ]);
            }
            else
            {
                TrackerBase.Say(x => x.Multiplayer.OtherPlayerClearedDungeonWithoutReward,
                    args: [args.PhoneticName, treasureRegion.Metadata.Name, boss.Region.RandomBossName]);
            }
        }
        else
        {
            TrackerBase.Say(x => x.Multiplayer.OtherPlayerDefeatedBoss, args: [args.PhoneticName, boss.RandomName]);
        }

    }

    private void PlayerTrackedDeath(PlayerTrackedDeathEventHandlerArgs args)
    {
        if (args.DeathLinkEnabled && !args.IsLocalPlayer)
        {
            Logger.LogInformation("Other player died with death link enabled");
            TrackerBase.GameService!.TryKillPlayer();
            TrackerBase.Say(x => x.Multiplayer.OtherPlayedDiedDeathLink, args: [args.PhoneticName]);
        }
        else if(!args.IsLocalPlayer)
        {
            TrackerBase.Say(x => x.Multiplayer.OtherPlayedDied, args: [args.PhoneticName]);
        }

    }

    private async void TrackerOnItemTracked(object? sender, ItemTrackedEventArgs e)
    {
        if (e.Item == null || e.Item.Type == ItemType.Nothing || !e.AutoTracked) return;

        if (e.Item.World.Guid == TrackerBase.World.Guid)
            await _multiplayerGameService.TrackItem(e.Item);
    }

    private async void TrackerOnBossUpdated(object? sender, BossTrackedEventArgs e)
    {
        if (e.Boss == null || e.Boss.Type == BossType.None || !e.AutoTracked) return;
        await _multiplayerGameService.TrackBoss(e.Boss);
    }

    private async void TrackerOnLocationCleared(object? sender, LocationClearedEventArgs e)
    {
        if (!e.AutoTracked) return;
        await _multiplayerGameService.TrackLocation(e.Location);
        if (e.Location.World == e.Location.Item.World || !e.Location.Item.Progression) return;
        var localItem = WorldQueryService.FirstOrDefault(e.Location.Item.Type);
        if (localItem == null || localItem.State.TrackingState >= e.Location.Item.State.TrackingState) return;
        var otherPlayer = e.Location.Item.World.Config.PhoneticName;
        TrackerBase.Say(x => x.Multiplayer.GiftedUsefulItemToOtherPlayer,
            args: [otherPlayer, localItem.Metadata.Name, localItem.Metadata.NameWithArticle]);
    }

    private async void TrackerOnBeatGame(object? sender, TrackerEventArgs e)
    {
        if (!e.AutoTracked) return;
        await _multiplayerGameService.CompletePlayerGame();
    }

    private async void TrackerOnPlayerDied(object? sender, TrackerEventArgs e)
    {
        if (!e.AutoTracked || TrackerBase.GameService!.PlayerRecentlyKilled) return;
        await _multiplayerGameService.TrackDeath();
    }

    public override void AddCommands()
    {

    }
}
