using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.Extensions.Logging;
using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Data.Configuration.ConfigTypes;
using TrackerCouncil.Smz3.Data.GeneratedData;
using TrackerCouncil.Smz3.Data.Logic;
using TrackerCouncil.Smz3.Data.Services;
using TrackerCouncil.Smz3.Data.Tracking;
using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.Data.WorldData.Regions;
using TrackerCouncil.Smz3.SeedGenerator.Contracts;
using TrackerCouncil.Smz3.SeedGenerator.Infrastructure;
using TrackerCouncil.Smz3.Shared;
using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.Tracking.Services;

namespace TrackerCouncil.Smz3.Tracking.TrackingServices;

internal class TrackerSpoilerService(IWorldQueryService worldQueryService, IGameHintService gameHintService, ILogger<TrackerSpoilerService> logger, IMetadataService metadataService, IPlayerProgressionService playerProgressionService, PlaythroughService playthroughService) : TrackerService, ITrackerSpoilerService
{
    /// <summary>
    /// Specifies the substrings that indicate a Super Metroid location name
    /// is a pretty worthless descriptor.
    /// </summary>
    private static readonly string[] s_worthlessLocationNameIndicators =
    [
        "behind", "top", "bottom", "middle"
    ];

    private readonly Dictionary<ItemType, int> _itemHintsGiven = new();
    private readonly Dictionary<LocationId, int> _locationHintsGiven = new();
    private Playthrough? _playthrough;
    private bool IsMultiworld => worldQueryService.World.Config.MultiWorld;

    public event EventHandler<TrackerEventArgs>? HintsToggled;
    public event EventHandler<TrackerEventArgs>? SpoilersToggled;

    public override void Initialize()
    {
        base.Initialize();

        var playerWorld = worldQueryService.World;
        Tracker.HintsEnabled = playerWorld.Config is { Race: false, DisableTrackerHints: false } && Tracker.Options.HintsEnabled;
        Tracker.SpoilersEnabled = playerWorld.Config is { Race: false, DisableTrackerSpoilers: false } && Tracker.Options.SpoilersEnabled;
        playthroughService.TryGenerate(new[] { playerWorld }, playerWorld.Config, out _playthrough);
    }

    public void ToggleHints(bool? newStatus, float? confidence = null)
    {
        var enabled = newStatus ?? !HintsEnabled;
        if (enabled != HintsEnabled)
        {
            Tracker.HintsEnabled = enabled;

            if (HintsEnabled)
            {
                Tracker.Say(x => x.Hints.EnabledHints);
            }
            else
            {
                Tracker.Say(x => x.Hints.DisabledHints);
            }

            HintsToggled?.Invoke(this, new TrackerEventArgs(confidence));

            AddUndo(() =>
            {
                Tracker.HintsEnabled = !enabled;

                if (HintsEnabled)
                {
                    Tracker.Say(x => x.Hints.EnabledHints);
                }
                else
                {
                    Tracker.Say(x => x.Hints.DisabledHints);
                }

                HintsToggled?.Invoke(this, new TrackerEventArgs(confidence));
            });
        }
        else
        {
            if (enabled)
            {
                Tracker.Say(x => x.Hints.AlreadyEnabledHints);
            }
            else
            {
                Tracker.Say(x => x.Hints.AlreadyDisabledHints);
            }
        }
    }

    public void ToggleSpoilers(bool? newStatus, float? confidence = null)
    {
        var enabled = newStatus ?? !SpoilersEnabled;
        if (enabled != SpoilersEnabled)
        {
            Tracker.SpoilersEnabled = enabled;

            if (SpoilersEnabled)
            {
                Tracker.Say(x => x.Spoilers.EnabledSpoilers);
            }
            else
            {
                Tracker.Say(x => x.Spoilers.DisabledSpoilers);
            }

            SpoilersToggled?.Invoke(this, new TrackerEventArgs(confidence));

            AddUndo(() =>
            {
                Tracker.SpoilersEnabled = !enabled;

                if (SpoilersEnabled)
                {
                    Tracker.Say(x => x.Spoilers.EnabledSpoilers);
                }
                else
                {
                    Tracker.Say(x => x.Spoilers.DisabledSpoilers);
                }

                SpoilersToggled?.Invoke(this, new TrackerEventArgs(confidence));
            });
        }
        else
        {
            if (enabled)
            {
                Tracker.Say(x => x.Spoilers.AlreadyEnabledSpoilers);
            }
            else
            {
                Tracker.Say(x => x.Spoilers.AlreadyDisabledSpoilers);
            }
        }
    }

    /// <summary>
    /// Gives a hint about where to go next.
    /// </summary>
    public void GiveProgressionHint()
    {
        if (!HintsEnabled && !SpoilersEnabled)
        {
            Tracker.Say(x => x.Hints.PromptEnableItemHints);
            return;
        }

        var locations = worldQueryService.Locations(keysanityByRegion: true).ToList();

        var result = gameHintService.FindMostValueableLocation(worldQueryService.Worlds, locations);

        if (result is not { Usefulness: LocationUsefulness.Mandatory or LocationUsefulness.Key})
        {
            Tracker.Say(x => x.Hints.NoApplicableHints);
            return;
        }

        Tracker.Say(x => x.Hints.AreaSuggestion, args: [result.Value.Location.Region.GetName()]);
    }

    public void GiveAreaHint(IHasLocations area, bool ignoreHintsIfSpoilersEnabled = false)
    {
        var responses = GetAreaHintResponse(area, ignoreHintsIfSpoilersEnabled);
        if (responses is { Successful: true, Responses.Count: > 0 })
        {
            Tracker.Say(preGeneratedDetails: responses);
        }
    }

    public TrackerResponseDetails GetAreaHintResponse(IHasLocations area, bool ignoreHintsIfSpoilersEnabled = false)
    {
        if (!HintsEnabled && !SpoilersEnabled)
        {
            return Tracker.GetTrackerResponses(x => x.Hints.PromptEnableItemHints);
        }

        var locations = area.Locations
            .Where(x => x.Cleared == false)
            .ToImmutableList();
        if (locations.Count == 0)
        {
            return Tracker.GetTrackerResponses(x => x.Hints.AreaAlreadyCleared, args: [area.GetName()]);
        }

        // Default to hints even if spoilers are enabled unless requested
        if (HintsEnabled && (!ignoreHintsIfSpoilersEnabled || !SpoilersEnabled))
        {
            if (area.Region!.Config.RomGenerator != RomGenerator.Cas)
            {
                var config = area.Region.Config;
                var anyPossibleProgression = area.Locations.Any(x =>
                    x.ItemType.IsPossibleProgression(config.ZeldaKeysanity, config.MetroidKeysanity, x.Item.IsLocalPlayerItem));
                if (anyPossibleProgression)
                {
                    return Tracker.GetTrackerResponses(x => x.Hints.AreaHasNonCasPossibleProgression, args: [area.GetName()]);
                }
                else if (area is IHasReward region && region.RewardType.IsInAnyCategory(RewardCategory.Crystal, RewardCategory.Metroid))
                {
                    return Tracker.GetTrackerResponses(x => x.Hints.AreaHasNonCasJunkAndReward, args: [area.GetName()]);
                }
                else
                {
                    return Tracker.GetTrackerResponses(x => x.Hints.AreaHasJunk, args: [area.GetName()]);
                }
            }

            var areaReward = GetRewardForHint(area);
            var usefulness = gameHintService.GetUsefulness(locations.ToList(), worldQueryService.Worlds, areaReward);

            if (usefulness == LocationUsefulness.Mandatory)
            {
                return Tracker.GetTrackerResponses(x => x.Hints.AreaHasSomethingMandatory, args: [area.GetName()]);
            }
            else if (usefulness == LocationUsefulness.Key)
            {
                return Tracker.GetTrackerResponses(x => x.Hints.AreaHasKey, args: [area.GetName()]);
            }
            else if (usefulness is LocationUsefulness.NiceToHave or LocationUsefulness.Sword)
            {
                return Tracker.GetTrackerResponses(x => x.Hints.AreaHasSomethingGood, args: [area.GetName()]);
            }
            else if (area is IHasReward region)
            {
                if (region.RewardType is RewardType.CrystalBlue or RewardType.CrystalRed)
                {
                    return Tracker.GetTrackerResponses(x => x.Hints.AreaHasJunkAndCrystal, args: [area.GetName()]);
                }
                else
                {
                    return Tracker.GetTrackerResponses(x => x.Hints.AreaHasJunk, args: [area.GetName()]);
                }
            }
            else
            {
                return Tracker.GetTrackerResponses(x => x.Hints.AreaHasJunk, args: [area.GetName()]);
            }
        }
        else if (SpoilersEnabled)
        {
            var items = locations
                .Select(x => x.Item)
                .NonNull();
            var itemNames = NaturalLanguage.Join(items, Tracker.World.Config);
            return Tracker.GetTrackerResponses(x => x.Spoilers.ItemsInArea, args: [area.GetName(), itemNames]);
        }

        return new TrackerResponseDetails { Successful = false };
    }

    public void RevealItemLocation(Item item, bool ignoreHintsIfSpoilersEnabled = false)
    {
        var response = GetItemLocationHintResponse(item, ignoreHintsIfSpoilersEnabled);
        if (response is { Successful: true, Responses.Count: > 0 })
        {
            Tracker.Say(preGeneratedDetails: response);
        }
    }

    public TrackerResponseDetails GetItemLocationHintResponse(Item item, bool ignoreHintsIfSpoilersEnabled = false)
    {
        if (item.Metadata.HasStages && item.TrackingState >= item.Metadata.MaxStage)
        {
            return Tracker.GetTrackerResponses(x => x.Spoilers.TrackedAllItemsAlready, args: [item.Name]);
        }
        else if (!item.Metadata.Multiple && item.TrackingState > 0)
        {
            return Tracker.GetTrackerResponses(x => x.Spoilers.TrackedItemAlready, args: [item.Metadata.NameWithArticle]);
        }

        var markedLocation = worldQueryService.MarkedLocations()
            .Where(x => x.MarkedItem == item.Type && !x.Cleared)
            .Random();
        if (markedLocation != null)
        {
            var locationName = markedLocation.Metadata.Name;
            var regionName = markedLocation.Region.Metadata.Name;
            return Tracker.GetTrackerResponses(x => x.Spoilers.MarkedItem, args: [item.Metadata.NameWithArticle, locationName, regionName]);
        }

        // Now that we're ready to give hints, make sure they're turned on
        if (!HintsEnabled && !SpoilersEnabled)
        {
            return Tracker.GetTrackerResponses(x => x.Hints.PromptEnableItemHints);
        }

        // Once we're done being a smartass, see if the item can be found at all
        var locations = worldQueryService.Locations(unclearedOnly: false, outOfLogic: true, itemFilter: item.Type, checkAllWorlds: true)
            .ToImmutableList();
        if (locations.Count == 0)
        {
            if (item.Metadata.Multiple || item.Metadata.HasStages)
                return Tracker.GetTrackerResponses(x => x.Spoilers.ItemsNotFound, args: [item.Metadata.Plural]);
            else
                return Tracker.GetTrackerResponses(x => x.Spoilers.ItemNotFound, args: [item.Metadata.NameWithArticle]);
        }

        // The item exists, but all locations are cleared
        else if (locations.Count > 0 && locations.All(x => x.Cleared))
        {
            // Prioritize locations that haven't been auto tracked
            var nonAutoTrackedLocations = locations.Where(x => !x.Autotracked).ToList();
            var locationsToAnnounce = nonAutoTrackedLocations.Count > 0 ? nonAutoTrackedLocations : locations.ToList();

            if (locationsToAnnounce.Count == 1)
            {
                return Tracker.GetTrackerResponses(x => x.Spoilers.ItemLocationCleared, args: [item.Metadata.NameWithArticle, locationsToAnnounce.First().RandomName]);
            }
            else
            {
                return Tracker.GetTrackerResponses(x => x.Spoilers.ItemLocationsCleared, args: [item.Metadata.NameWithArticle, NaturalLanguage.Join(locationsToAnnounce)]);
            }
        }

        // Default to hints even if spoilers are enabled unless requested
        if (HintsEnabled && (!ignoreHintsIfSpoilersEnabled || !SpoilersEnabled))
        {
            var response = GiveItemLocationHint(item);
            if (response is { Successful: true, Responses.Count: > 0 })
            {
                return response;
            }
            else
            {
                logger.LogWarning("Ran out of hints for {Item}", item.Name);
                return Tracker.GetTrackerResponses(x => x.Hints.NoApplicableHints);
            }
        }

        // Fall back to spoilers if enabled, or prompt to turn them on if hints fail
        else if (SpoilersEnabled)
        {
            return GiveItemLocationSpoiler(item);
        }
        else
        {
            return Tracker.GetTrackerResponses(x => x.Spoilers.PromptEnableItemSpoilers);
        }
    }

    public void RevealLocationItem(Location location, bool ignoreHintsIfSpoilersEnabled = false)
    {
        var response = GetLocationItemHintResponse(location, ignoreHintsIfSpoilersEnabled);
        if (response is { Successful: true, Responses.Count: > 0 })
        {
            Tracker.Say(preGeneratedDetails: response);
        }
    }

    public TrackerResponseDetails GetLocationItemHintResponse(Location location, bool ignoreHintsIfSpoilersEnabled = false)
    {
        var locationName = location.Metadata.Name;
        if (location.Cleared)
        {
            if (HintsEnabled || SpoilersEnabled)
            {
                var itemName = metadataService.GetName(location.Item.Type);
                return Tracker.GetTrackerResponses(x => x.Hints.LocationAlreadyClearedSpoiler, args: [locationName, itemName]);
            }
            else
            {
                return Tracker.GetTrackerResponses(x => x.Hints.LocationAlreadyCleared, args: [locationName]);
            }
        }

        if (location.MarkedItem != null)
        {
            var markedItem = worldQueryService.FirstOrDefault(location.MarkedItem.Value);
            if (markedItem != null)
            {
                return Tracker.GetTrackerResponses(x => x.Spoilers.MarkedLocation, args: [locationName, markedItem.Metadata.NameWithArticle]);
            }
        }

        // Default to hints even if spoilers are enabled unless requested
        if (HintsEnabled && (!ignoreHintsIfSpoilersEnabled || !SpoilersEnabled))
        {
            var response = GiveLocationHints(location);
            if (response is { Successful: true, Responses.Count: > 0 })
            {
                return response;
            }
            else
            {
                return Tracker.GetTrackerResponses(x => x.Spoilers.PromptEnableLocationSpoilers);
            }
        }
        else if (SpoilersEnabled)
        {
            return GiveLocationSpoiler(location);
        }
        else
        {
            return Tracker.GetTrackerResponses(x => x.Hints.PromptEnableLocationHints);
        }
    }

    /// <summary>
    /// Gives the specified item hint for the specified item.
    /// </summary>
    /// <param name="selectHint">Selects the hint to give.</param>
    /// <param name="item">The item that was asked about.</param>
    /// <param name="additionalArgs">
    /// ADditional arguments used to format the hint text.
    /// </param>
    /// <returns>
    /// <c>true</c> if a hint was given. <c>false</c> if the selected hint
    /// was null, empty or returned a <c>null</c> hint.
    /// </returns>
    private TrackerResponseDetails GiveItemHint(Func<HintsConfig, SchrodingersString?> selectHint,
        Item item, params object?[] additionalArgs)
    {
        var args = Args.Combine(item.Metadata.NameWithArticle, additionalArgs);
        var response = Tracker.GetTrackerResponses(responses => selectHint(responses.Hints), args: args);
        if (response.Successful)
        {
            if (_itemHintsGiven.ContainsKey(item.Type))
                _itemHintsGiven[item.Type]++;
            else
                _itemHintsGiven[item.Type] = 1;
        }
        return response;
    }

    /// <summary>
    /// Gives the specified item hint for the specified item.
    /// </summary>
    /// <param name="hint">The hint to give.</param>
    /// <param name="item">The item that was asked about.</param>
    /// <param name="additionalArgs">
    /// Additional arguments used to format the hint text.
    /// </param>
    /// <returns>
    /// <c>true</c> if a hint was given. <c>false</c> if <paramref
    /// name="hint"/> was null, empty or returned a <c>null</c> hint.
    /// </returns>
    private TrackerResponseDetails GiveItemHint(SchrodingersString? hint,
        Item item, params object?[] additionalArgs)
    {
        var args = Args.Combine(item.Metadata.NameWithArticle, additionalArgs);
        var response = Tracker.GetTrackerResponses(response: hint, args: args);
        if (response.Successful)
        {
            if (_itemHintsGiven.ContainsKey(item.Type))
                _itemHintsGiven[item.Type]++;
            else
                _itemHintsGiven[item.Type] = 1;
        }
        return response;
    }

    /// <summary>
    /// Gives the specified location hint for the specified item.
    /// </summary>
    /// <param name="selectHint">Selects the hint to give.</param>
    /// <param name="location">The lcoation that was asked about.</param>
    /// <param name="additionalArgs">
    /// ADditional arguments used to format the hint text.
    /// </param>
    /// <returns>
    /// <c>true</c> if a hint was given. <c>false</c> if the selected hint
    /// was null, empty or returned a <c>null</c> hint.
    /// </returns>
    private TrackerResponseDetails GiveLocationHint(Func<HintsConfig, SchrodingersString?> selectHint,
        Location location, params object?[] additionalArgs)
    {
        var name = location.Metadata.Name;
        var args = Args.Combine(name, additionalArgs);
        var details = Tracker.GetTrackerResponses(responses => selectHint(responses.Hints), args: args);
        if (details.Successful)
        {
            if (_locationHintsGiven.ContainsKey(location.Id))
                _locationHintsGiven[location.Id]++;
            else
                _locationHintsGiven[location.Id] = 1;
        }
        return details;
    }

    /// <summary>
    /// Gives the specified location hint for the specified item.
    /// </summary>
    /// <param name="hint">The hint to give.</param>
    /// <param name="location">The lcoation that was asked about.</param>
    /// <param name="additionalArgs">
    /// ADditional arguments used to format the hint text.
    /// </param>
    /// <returns>
    /// <c>true</c> if a hint was given. <c>false</c> if the selected hint
    /// was null, empty or returned a <c>null</c> hint.
    /// </returns>
    private TrackerResponseDetails GiveLocationHint(SchrodingersString? hint,
        Location location, params object?[] additionalArgs)
    {
        var name = location.Metadata.Name;
        var args = Args.Combine(name, additionalArgs);
        var details = Tracker.GetTrackerResponses(response: hint, args: args);
        if (details.Successful)
        {
            if (_locationHintsGiven.ContainsKey(location.Id))
                _locationHintsGiven[location.Id]++;
            else
                _locationHintsGiven[location.Id] = 1;
        }
        return details;
    }

    private Reward? GetRewardForHint(IHasLocations area)
    {
        if (area is not IHasReward rewardArea || !rewardArea.RewardType.IsInAnyCategory(RewardCategory.Crystal, RewardCategory.Pendant) ||
            rewardArea.RewardType == RewardType.None || area is not IHasBoss bossRegion) return null;

        var bossLocation = area.Locations.FirstOrDefault(x => x.Id == bossRegion.BossLocationId);

        if (bossLocation == null)
        {
            return null;
        }

        // For pendant dungeons, only factor it in if the player has not gotten it so that they get hints
        // that factor in Saha/Ped
        if (rewardArea.RewardType.IsInCategory(RewardCategory.Pendant) && (bossLocation.Cleared || bossLocation.Autotracked))
        {
            return rewardArea.Reward;
        }
        // For crystal dungeons, always act like the player has them so that it only gives a hint based on
        // the actual items in the dungeon
        else if (rewardArea.RewardType.IsInCategory(RewardCategory.Crystal))
        {
            return rewardArea.Reward;
        }

        return null;
    }

    private TrackerResponseDetails GiveLocationHints(Location location)
    {
        if (location.World.Config.RomGenerator != RomGenerator.Cas)
        {
            if (HintsGiven(location) == 0)
            {
                if (location.ItemType.IsPossibleProgression(location.World.Config.ZeldaKeysanity,
                        location.World.Config.MetroidKeysanity, location.Item.IsLocalPlayerItem))
                {
                    return GiveLocationHint(x => x.LocationHasNonCasProgressionItem, location, location.Item.PlayerName);
                }
                else
                {
                    return GiveLocationHint(x => x.LocationHasJunkItem, location, location.Item.PlayerName);
                }
            }

            return new TrackerResponseDetails { Successful = false };
        }

        switch (HintsGiven(location))
        {
            // Who's it for and is it any good?
            case 0:

                var areaReward = GetRewardForHint(location.Region);
                var usefulness = gameHintService.GetUsefulness([ location ], worldQueryService.Worlds, areaReward);

                var characterName = location.Item.Type.IsInCategory(ItemCategory.Metroid)
                    ? TrackerBase.CorrectPronunciation(location.World.Config.SamusName)
                    : TrackerBase.CorrectPronunciation(location.World.Config.LinkName);

                switch (usefulness)
                {
                    case LocationUsefulness.Mandatory:
                        return GiveLocationHint(x => x.LocationHasMandatoryItem, location, characterName);
                    case LocationUsefulness.Key:
                        return GiveLocationHint(x => x.LocationHasKey, location, characterName);
                    case LocationUsefulness.NiceToHave or LocationUsefulness.Sword:
                        return GiveLocationHint(x => x.LocationHasUsefulItem, location, characterName);
                    default:
                        return GiveLocationHint(x => x.LocationHasJunkItem, location, characterName);
                }

            // Try to give a hint from the config
            case 1:
                var hint = location.Item.Metadata.Hints;
                return hint is { Count: > 0 } ? GiveLocationHint(hint, location) : GiveLocationHint(x => x.NoApplicableHints, location);

            // Consult the Book of Mudora
            case 2:
                var pedText = location.Item.Metadata.PedestalHints;
                var bookOfMudoraName = metadataService.GetName(ItemType.Book);
                return GiveLocationHint(x => x.BookHint, location, pedText, bookOfMudoraName);
        }

        return new TrackerResponseDetails { Successful = false };
    }

    private TrackerResponseDetails GiveLocationSpoiler(Location location)
    {
        var locationName = location.Metadata.Name;
        if (location.Item.Type == ItemType.Nothing)
        {
            return Tracker.GetTrackerResponses(x => x.Spoilers.EmptyLocation, args: [locationName]);
        }

        if (location.World.Config.RomGenerator != RomGenerator.Cas)
        {
            if (location.Item.Type is ItemType.OtherGameItem or ItemType.OtherGameProgressionItem)
            {
                return Tracker.GetTrackerResponses(x => x.Spoilers.LocationHasOtherGameItem,
                    args: [
                        locationName,
                        location.Item.OriginalName,
                        location.Item.PlayerName
                    ]);
            }
            else
            {
                return Tracker.GetTrackerResponses(x => location.Item.IsLocalPlayerItem ? x.Spoilers.LocationHasItemOwnWorld : x.Spoilers.LocationHasItemOtherWorld,
                    args: [
                        locationName,
                        location.Item.Metadata.NameWithArticle,
                        location.Item.PlayerName
                    ]);
            }
        }

        var item = location.Item;
        if (item.Type != ItemType.Nothing)
        {
            if (IsMultiworld)
            {
                return Tracker.GetTrackerResponses(x => location.Item.World == worldQueryService.World ? x.Spoilers.LocationHasItemOwnWorld : x.Spoilers.LocationHasItemOtherWorld,
                    args: [
                        locationName,
                        item.Metadata.NameWithArticle,
                        location.Item.World.Player
                    ]);
            }
            else
            {
                return Tracker.GetTrackerResponses(x => x.Spoilers.LocationHasItem, args: [locationName, item.Metadata.NameWithArticle]);
            }
        }
        else
        {
            return Tracker.GetTrackerResponses(x => x.Spoilers.LocationHasUnknownItem, args: [locationName]);
        }
    }

    private TrackerResponseDetails GiveItemLocationSpoiler(Item item)
    {
        if (item.Metadata == null)
            throw new InvalidOperationException($"No metadata for item '{item.Name}'");

        List<TrackerResponseLine>? lines = null;

        var reachableLocation = worldQueryService.Locations(itemFilter: item.Type, keysanityByRegion: true, checkAllWorlds: true)
            .Random();
        if (reachableLocation != null)
        {
            var locationName = reachableLocation.Metadata.Name;
            var regionName = reachableLocation.Region.Metadata.Name;

            if (IsMultiworld)
            {
                if (item.Metadata.Multiple || item.Metadata.HasStages)
                {

                    return Tracker.GetTrackerResponses(x => reachableLocation.World == worldQueryService.World ? x.Spoilers.ItemsAreAtLocationOwnWorld : x.Spoilers.ItemsAreAtLocationOtherWorld,
                        args: [
                            item.Metadata.NameWithArticle,
                            locationName,
                            regionName,
                            reachableLocation.World.Player
                        ]);
                }
                else
                {
                    return Tracker.GetTrackerResponses(x => reachableLocation.World == worldQueryService.World ? x.Spoilers.ItemIsAtLocationOwnWorld : x.Spoilers.ItemIsAtLocationOtherWorld,
                        args: [
                            item.Metadata.NameWithArticle,
                            locationName,
                            regionName,
                            reachableLocation.World.Player
                        ]);
                }
            }
            else
            {
                if (item.Metadata.Multiple || item.Metadata.HasStages)
                    return Tracker.GetTrackerResponses(x => x.Spoilers.ItemsAreAtLocation, args: [item.Metadata.NameWithArticle, locationName, regionName]);
                else
                    return Tracker.GetTrackerResponses(x => x.Spoilers.ItemIsAtLocation, args: [item.Metadata.NameWithArticle, locationName, regionName]);
            }
        }

        var worldLocation = worldQueryService.Locations(outOfLogic: true, itemFilter: item.Type, checkAllWorlds: true)
            .Random();
        if (worldLocation != null)
        {
            var locationName = worldLocation.Metadata.Name;
            var regionName = worldLocation.Region.Metadata.Name;

            if (IsMultiworld)
            {
                if (item.Metadata.Multiple || item.Metadata.HasStages)
                {
                    return Tracker.GetTrackerResponses(x => worldLocation.World == worldQueryService.World ? x.Spoilers.ItemsAreAtOutOfLogicLocationOwnWorld : x.Spoilers.ItemsAreAtOutOfLogicLocationOtherWorld,
                        args: [
                            item.Metadata.NameWithArticle,
                            locationName,
                            regionName,
                            worldLocation.World.Player
                        ]);
                }
                else
                {
                    return Tracker.GetTrackerResponses(x => worldLocation.World == worldQueryService.World ? x.Spoilers.ItemIsAtOutOfLogicLocationOwnWorld : x.Spoilers.ItemIsAtOutOfLogicLocationOtherWorld,
                        args: [
                            item.Metadata.NameWithArticle,
                            locationName,
                            regionName,
                            worldLocation.World.Player
                        ]);
                }
            }
            else
            {
                if (item.Metadata.Multiple || item.Metadata.HasStages)
                    return Tracker.GetTrackerResponses(x => x.Spoilers.ItemsAreAtOutOfLogicLocation, args: [item.Metadata.NameWithArticle, locationName, regionName]);
                else
                    return Tracker.GetTrackerResponses(x => x.Spoilers.ItemIsAtOutOfLogicLocation, args: [item.Metadata.NameWithArticle, locationName, regionName]);
            }
        }

        return new TrackerResponseDetails { Successful = false };
    }

    private TrackerResponseDetails GiveItemLocationHint(Item item)
    {
        var itemLocations = worldQueryService.Locations(outOfLogic: true, itemFilter: item.Type, checkAllWorlds: true).ToList();

        if (!itemLocations.Any())
        {
            logger.LogInformation("Can't find any uncleared locations with {Item}", item.Type.GetDescription());
            return null;
        }

        switch (HintsGiven(item))
        {
            // Lv0 hints are the vaguest:
            // - Is it out of logic?
            // - Is it only in one game (mostly useful for progression
            // items)
            // - Is it in another player's world?
            case 0:
                {

                    var isInLogic = itemLocations.Any(x => x.IsRelevant(playerProgressionService.GetProgression(x.Region)) && x.World.IsLocalWorld);
                    if (isInLogic)
                    {
                        var isOnlyInSuperMetroid = itemLocations.Select(x => x.Region).All(x => x is SMRegion);
                        if (isOnlyInSuperMetroid)
                            return GiveItemHint(x => x.ItemInSuperMetroid, item);

                        var isOnlyInALinkToThePast = itemLocations.Select(x => x.Region).All(x => x is Z3Region);
                        if (isOnlyInALinkToThePast)
                            return GiveItemHint(x => x.ItemInALttP, item);
                    }
                    else
                    {
                        var isInOtherWorld = itemLocations.Any(x => !x.World.IsLocalWorld);
                        if (isInOtherWorld)
                            return GiveItemHint(x => x.ItemInUnknownWorld, item);
                        return GiveItemHint(x => x.ItemNotInLogic, item);
                    }

                    return GiveItemHint(x => x.NoApplicableHints, item);
                }

            // Lv1 hints are still pretty vague:
            // - Is it early or late?
            // - Where will you NOT find it?
            // - Exactly which player's world is the item in?
            case 1:
                {
                    if (itemLocations.All(x => !x.IsRelevant(playerProgressionService.GetProgression(x.Region))))
                    {
                        var randomLocation = itemLocations.Where(x => !x.IsRelevant(playerProgressionService.GetProgression(x.Region))).Random();

                        if (randomLocation == null)
                        {
                            return GiveItemHint(x => x.NoApplicableHints, item);
                        }

                        if (!randomLocation.World.IsLocalWorld)
                        {
                            return GiveItemHint(x => x.ItemInPlayerWorld, item,
                                randomLocation.World.Config.PhoneticName);
                        }

                        var progression = playerProgressionService.GetProgression(randomLocation.Region);
                        var missingItemSets = Logic.GetMissingRequiredItems(randomLocation, progression, out _);
                        if (!missingItemSets.Any())
                        {
                            return GiveItemHint(x => x.ItemRequiresManyOtherItems, item);
                        }
                        else
                        {
                            var randomMissingItem = Logic.GetMissingRequiredItems(randomLocation, progression, out _)
                                .SelectMany(x => x)
                                .Where(x => x != item.Type)
                                .Select(x => worldQueryService.FirstOrDefault(x))
                                .Random();
                            if (randomMissingItem != null)
                                return GiveItemHint(x => x.ItemRequiresOtherItem, item, randomMissingItem.Metadata.NameWithArticle);
                        }
                    }

                    var otherWorldLocation = itemLocations.FirstOrDefault(x => !x.World.IsLocalWorld);
                    if (otherWorldLocation != null)
                    {
                        return GiveItemHint(x => x.ItemInPlayerWorld, item,
                            otherWorldLocation.World.Config.PhoneticName);
                    }


                    if (_playthrough == null)
                    {
                        return GiveItemHint(x => x.PlaythroughImpossible, item);
                    }

                    var sphere = _playthrough.Spheres.IndexOf(x => x.Items.Any(i => i.Type == item.Type));
                    var earlyThreshold = Math.Floor(_playthrough.Spheres.Count * 0.25);
                    var lateThreshold = Math.Ceiling(_playthrough.Spheres.Count * 0.75);
                    logger.LogDebug("{Item} spoiler: sphere {Sphere}, early: {Early}, late: {Late}", item, sphere, earlyThreshold, lateThreshold);
                    if (sphere == 0)
                        return GiveItemHint(x => x.ItemInSphereZero, item);
                    if (sphere < earlyThreshold)
                        return GiveItemHint(x => x.ItemInEarlySphere, item);
                    if (sphere > lateThreshold)
                        return GiveItemHint(x => x.ItemInLateSphere, item);

                    var areaWithoutItem = Tracker.World.Regions
                        .GroupBy(x => x.Area)
                        .Where(x => x.SelectMany(r => r.Locations)
                            .Where(l => l is { Cleared: false, Autotracked: false })
                            .All(l => l.Item.Type != item.Type))
                        .OrderByDescending(x =>
                            x.SelectMany(r => r.Locations).Count(l => l is { Cleared: false, Autotracked: false, Accessibility: Accessibility.Available or Accessibility.AvailableWithKeys }))
                        .Select(x => x.Key)
                        .FirstOrDefault();
                    if (areaWithoutItem != null)
                        return GiveItemHint(x => x.ItemNotInArea, item, areaWithoutItem);

                    return GiveItemHint(x => x.NoApplicableHints, item);
                }

            // Lv2 hints are less vague still: have you there or not?
            // - Is it in a dungeon? Have you been there before or not?
            // - Is it in an area you've been before?
            // - Is it in another player's ALttP or SM?
            case 2:
                {
                    var randomLocation = GetRandomItemLocationWithFilter(item, _ => true);

                    if (randomLocation?.World.IsLocalWorld == false)
                    {
                        if (randomLocation.Region is Z3Region)
                            return GiveItemHint(x => x.ItemInPlayerWorldALttP, item,
                                randomLocation.World.Config.PhoneticName);
                        else if (randomLocation.Region is SMRegion)
                            return GiveItemHint(x => x.ItemInPlayerWorldSuperMetroid, item,
                                randomLocation.World.Config.PhoneticName);
                    }

                    if (randomLocation?.Region is Z3Region and IHasReward dungeon && dungeon.RewardType != RewardType.Agahnim)
                    {
                        if (randomLocation.Region.Locations.Any(x => x.Cleared))
                            return GiveItemHint(x => x.ItemInPreviouslyVisitedDungeon, item);
                        else
                            return GiveItemHint(x => x.ItemInUnvisitedDungeon, item);
                    }

                    if (randomLocation?.Region?.Locations.Any(x => x.Cleared) == true)
                        return GiveItemHint(x => x.ItemInPreviouslyVisitedRegion, item);
                    else
                        return GiveItemHint(x => x.ItemInUnvisitedRegion, item);
                }

            // Lv3 hints give a clue about the actual region it is in.
            case 3:
                {
                    var randomLocationWithHint = GetRandomItemLocationWithFilter(item,
                        l => l.Region.Metadata.Hints?.Count > 0);
                    if (randomLocationWithHint != null)
                    {
                        var regionHint = randomLocationWithHint.Region.Metadata.Hints;
                        if (regionHint is { Count: > 0 })
                        {
                            if (!randomLocationWithHint.World.IsLocalWorld)
                                Tracker.Say(x => x.Hints.ItemInPlayerWorldRegionRoomPrefixHint,
                                    args: [randomLocationWithHint.World.Config.PhoneticName]);
                            return GiveItemHint(regionHint, item);
                        }
                    }

                    return GiveItemHint(x => x.NoApplicableHints, item);
                }

            // Lv4 hints give a clue about the room or location itself
            case 4:
                {
                    var randomLocation = GetRandomItemLocationWithFilter(item,
                        location =>
                        {
                            if (location.Room is { Metadata.Hints.Count: > 0 })
                                return true;
                            return location.Metadata.Hints?.Count > 0;
                        });

                    if (randomLocation != null)
                    {
                        if (randomLocation.Room != null)
                        {
                            var roomHint = randomLocation.Room.Metadata.Hints;
                            if (roomHint is { Count: > 0 })
                            {
                                if (!randomLocation.World.IsLocalWorld)
                                    Tracker.Say(x => x.Hints.ItemInPlayerWorldRegionRoomPrefixHint,
                                        args: [randomLocation.World.Config.PhoneticName]);
                                return GiveItemHint(roomHint, item);
                            }
                        }

                        var locationHint = randomLocation.Metadata.Hints;
                        if (locationHint is { Count: > 0 })
                        {
                            if (!randomLocation.World.IsLocalWorld)
                                Tracker.Say(x => x.Hints.ItemInPlayerWorldRegionRoomPrefixHint,
                                    args: [randomLocation.World.Config.PhoneticName]);
                            return GiveItemHint(locationHint, item);
                        }
                    }

                    randomLocation = GetRandomItemLocationWithFilter(item, x => x.VanillaItem.IsInCategory(ItemCategory.Plentiful));
                    if (randomLocation != null && randomLocation.VanillaItem.IsInCategory(ItemCategory.Plentiful))
                    {
                        if (!randomLocation.World.IsLocalWorld)
                            Tracker.Say(x => x.Hints.ItemInPlayerWorldRegionRoomPrefixHint,
                                args: [randomLocation.World.Config.PhoneticName]);

                        if (randomLocation.Region is SMRegion
                            && randomLocation.Name.ContainsAny(StringComparison.OrdinalIgnoreCase, s_worthlessLocationNameIndicators))
                        {
                            // Just give the name of the location from the
                            // original SMZ3 randomizer code, it's vague
                            // enough
                            return GiveItemHint(x => x.ItemHasBadVanillaLocationName, item, randomLocation.Name);
                        }

                        var vanillaItem = worldQueryService.FirstOrDefault(randomLocation.VanillaItem);
                        return GiveItemHint(x => x.ItemIsInVanillaJunkLocation, item, vanillaItem?.Metadata.Name ?? randomLocation.VanillaItem.GetDescription());
                    }

                    // If there isn't any location with this item that has a
                    // hint, let it fall through so tracker can tell the
                    // player to enable spoilers
                    logger.LogInformation("No more hints for {Item}", item);
                }
                break;
        }

        return null;
    }

    private int HintsGiven(Item item) => _itemHintsGiven.GetValueOrDefault(item.Type, 0);

    private int HintsGiven(Location location) => _locationHintsGiven.GetValueOrDefault(location.Id, 0);

    private Location? GetRandomItemLocationWithFilter(Item item, Func<Location, bool> predicate)
    {
        var randomLocation = worldQueryService.Locations(itemFilter: item.Type, keysanityByRegion: true, checkAllWorlds: true)
            .Where(predicate)
            .Random();

        if (randomLocation == null)
        {
            // If the item is not at any accessible location, try to look in
            // out-of-logic places, too.
            randomLocation = worldQueryService.Locations(outOfLogic: true,  itemFilter: item.Type, keysanityByRegion: true, checkAllWorlds: true)
                .Where(predicate)
                .Random();
        }

        return randomLocation;
    }
}
