using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using SnesConnectorLibrary;
using SnesConnectorLibrary.Requests;
using SNI;
using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Data.Tracking;
using TrackerCouncil.Smz3.Data.WorldData.Regions;
using TrackerCouncil.Smz3.Data.WorldData.Regions.SuperMetroid;
using TrackerCouncil.Smz3.Data.WorldData.Regions.SuperMetroid.Brinstar;
using TrackerCouncil.Smz3.Data.WorldData.Regions.SuperMetroid.Maridia;
using TrackerCouncil.Smz3.Data.WorldData.Regions.SuperMetroid.Norfair;
using TrackerCouncil.Smz3.Shared.Enums;

namespace TrackerCouncil.Smz3.Tracking.AutoTracking.MetroidStateChecks;

/// <summary>
/// State for when viewing the Metroid pause map for randomized rewards in parsed AP/Mainline games
/// </summary>
public class ViewedMetroidMap(ISnesConnectorService snesConnectorService, ILogger<ViewedMetroidMap> logger) : IMetroidStateCheck
{
    private bool _isMakingRequest;
    private readonly List<SMRegion> _viewedMaps = [];

    /// <summary>
    /// Executes the check for the current state
    /// </summary>
    /// <param name="tracker">The tracker instance</param>
    /// <param name="currentState">The current state in Super Metroid</param>
    /// <param name="prevState">The previous state in Super Metroid</param>
    /// <returns>True if the check was identified, false otherwise</returns>
    public bool ExecuteCheck(TrackerBase tracker, AutoTrackerMetroidState currentState, AutoTrackerMetroidState prevState)
    {
        if (tracker.World.Config.RomGenerator == RomGenerator.Cas || !currentState.IsLookingAtMap || _isMakingRequest || tracker.GameStateTracker.CurrentRegion is not SMRegion region || _viewedMaps.Contains(region))
        {
            return false;
        }

        _isMakingRequest = true;
        snesConnectorService.MakeMemoryRequest(new SnesSingleMemoryRequest
        {
            MemoryRequestType = SnesMemoryRequestType.RetrieveMemory,
            SnesMemoryDomain = SnesMemoryDomain.ConsoleRAM,
            AddressFormat = AddressFormat.Snes9x,
            SniMemoryMapping = MemoryMapping.ExHiRom,
            Address = 0x7E00B1,
            Length = 4,
            OnResponse = (currentData, _) =>
            {
                var x = currentData.ReadUInt16(0);
                var y = currentData.ReadUInt16(2);
                if (x == null || y == null)
                {
                    return;
                }
                CheckRegion(tracker, region, x.Value, y.Value);
            }
        });

        return true;
    }

    private void CheckRegion(TrackerBase tracker, SMRegion region, int x, int y)
    {
        var needsMap = region.Config.MetroidKeysanity;
        var world = tracker.World;

        logger.LogInformation("{X},{Y}", x, y);

        if (region is KraidsLair or BlueBrinstar or GreenBrinstar or RedBrinstar && x is >= 204 and <= 276 && y is <= 72 or >= 65520 && (!needsMap || tracker.PlayerProgressionService.IsTracked(ItemType.SmMapBrinstar)))
        {
            tracker.RewardTracker.SetAreaReward(world.KraidsLair, world.KraidsLair.RewardState.RewardType);
            _viewedMaps.AddRange([world.KraidsLair, world.BlueBrinstar, world.GreenBrinstar, world.RedBrinstar]);
        }
        else if (region is WreckedShip && y is <= 48 or >= 65520 && (!needsMap || tracker.PlayerProgressionService.IsTracked(ItemType.SmMapWreckedShip)))
        {
            tracker.RewardTracker.SetAreaReward(world.WreckedShip, world.WreckedShip.RewardState.RewardType);
            _viewedMaps.Add(region);
        }
        else if (region is InnerMaridia or OuterMaridia && x is >= 72 and <= 204 && y is <= 48 or >= 65480 && (!needsMap || tracker.PlayerProgressionService.IsTracked(ItemType.SmMapMaridia)))
        {
            tracker.RewardTracker.SetAreaReward(world.InnerMaridia, world.InnerMaridia.RewardState.RewardType);
            _viewedMaps.AddRange([world.InnerMaridia, world.OuterMaridia]);
        }
        else if (region is UpperNorfairEast or UpperNorfairWest or UpperNorfairCrocomire or LowerNorfairEast
                 or LowerNorfairWest && y is <= 72 or >= 65504 && (!needsMap || tracker.PlayerProgressionService.IsTracked(ItemType.SmMapLowerNorfair)))
        {
            tracker.RewardTracker.SetAreaReward(world.LowerNorfairEast, world.LowerNorfairEast.RewardState.RewardType);
            _viewedMaps.AddRange([world.UpperNorfairEast, world.UpperNorfairWest, world.UpperNorfairCrocomire, world.LowerNorfairEast, world.LowerNorfairWest]);
        }

        _isMakingRequest = false;
    }
}
