using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Randomizer.Multiplayer.Client;
using Randomizer.Shared;
using Randomizer.Shared.Models;
using Randomizer.SMZ3.Tracking.Services;

namespace Randomizer.SMZ3.Tracking.VoiceCommands;

public class MultiplayerModule : TrackerModule
{
    private readonly MultiplayerGameService _multiplayerGameService;

    /// <summary>
    /// Initializes a new instance of the <see cref="MultiplayerModule"/>
    /// class.
    /// </summary>
    /// <param name="tracker">The tracker instance.</param>
    /// <param name="itemService">Service to get item information</param>
    /// <param name="worldService">Service to get world information</param>
    /// <param name="logger">Used to write logging information.</param>
    /// <param name="multiplayerGameService">The multiplayer game service</param>
    public MultiplayerModule(Tracker tracker, IItemService itemService, IWorldService worldService,
        ILogger<MultiplayerModule> logger, MultiplayerGameService multiplayerGameService)
        : base(tracker, itemService, worldService, logger)
    {
        _multiplayerGameService = multiplayerGameService;

        if (Tracker.Rom is { MultiplayerGameDetails: null }) return;

        Tracker.LocationCleared += TrackerOnLocationCleared;
        Tracker.BossUpdated += TrackerOnBossUpdated;
        Tracker.ItemTracked += TrackerOnItemTracked;
        Tracker.DungeonUpdated += TrackerOnDungeonUpdated;

        _multiplayerGameService.PlayerTrackedLocation += PlayerTrackedLocation;
        _multiplayerGameService.PlayerTrackedItem += PlayerTrackedItem;
        _multiplayerGameService.PlayerTrackedBoss += PlayerTrackedBoss;
        _multiplayerGameService.PlayerTrackedDungeon += PlayerTrackedDungeon;
        _multiplayerGameService.PlayerSyncReceived += PlayerSyncReceived;

        _multiplayerGameService.SetTrackerState(worldService.World.State!);

        _multiplayerGameService.OnTrackingStarted();

        Logger.LogInformation("Multiplayer module initialized");
    }

    private void PlayerSyncReceived(PlayerSyncReceivedEventHandlerArgs args)
    {
        if (args.PlayerId == null || args.ItemsToGive == null || args.ItemsToGive.Count == 0) return;
        var items = args.ItemsToGive.Select(x => ItemService.FirstOrDefault(x)).NonNull().ToList();
        Tracker.GameService!.TryGiveItems(items, args.PlayerId.Value);
        SaveTracker();

    }

    private void PlayerTrackedItem(PlayerTrackedItemEventHandlerArgs args)
    {

    }

    private void PlayerTrackedLocation(PlayerTrackedLocationEventHandlerArgs args)
    {
        if (args.ItemToGive == ItemType.Nothing) return;
        var item = ItemService.FirstOrDefault(args.ItemToGive);
        if (item == null)
            throw new InvalidOperationException($"Player retrieved invalid item {args.ItemToGive}");
        Tracker.GameService!.TryGiveItem(item, args.PlayerId);
        SaveTracker();
    }

    private void PlayerTrackedDungeon(PlayerTrackedDungeonEventHandlerArgs args)
    {

    }

    private void PlayerTrackedBoss(PlayerTrackedBossEventHandlerArgs args)
    {

    }

    private async void TrackerOnDungeonUpdated(object? sender, DungeonTrackedEventArgs e)
    {
        if (e.Dungeon == null || !e.AutoTracked || !e.Dungeon.DungeonState.Cleared) return;
        await _multiplayerGameService.TrackDungeon(e.Dungeon);
        SaveTracker();
    }

    private async void TrackerOnItemTracked(object? sender, ItemTrackedEventArgs e)
    {
        if (e.Item == null || !e.AutoTracked) return;

        if (e.Item.World.Guid == Tracker.World.Guid)
            await _multiplayerGameService.TrackItem(e.Item);
        SaveTracker();
    }

    private async void TrackerOnBossUpdated(object? sender, BossTrackedEventArgs e)
    {
        if (e.Boss == null || !e.AutoTracked) return;
        await _multiplayerGameService.TrackBoss(e.Boss);
        SaveTracker();
    }

    private async void TrackerOnLocationCleared(object? sender, LocationClearedEventArgs e)
    {
        if (!e.AutoTracked) return;
        await _multiplayerGameService.TrackLocation(e.Location);
        SaveTracker();
    }

    /// <summary>
    /// Monitor task tp force disconnect the socket if we haven't received a message within the last 5 seconds
    /// </summary>
    private void SaveTracker()
    {
        if (GeneratedRom.IsValid(Tracker.Rom))
        {
            Task.Run(() => Tracker.SaveAsync(Tracker.Rom));
        }
    }

}
