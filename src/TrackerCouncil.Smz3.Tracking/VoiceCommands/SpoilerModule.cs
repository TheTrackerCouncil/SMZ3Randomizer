using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.Versioning;
using Microsoft.Extensions.Logging;
using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Data.Configuration.ConfigTypes;
using TrackerCouncil.Smz3.Data.GeneratedData;
using TrackerCouncil.Smz3.Data.Logic;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Data.Services;
using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.Data.WorldData.Regions;
using TrackerCouncil.Smz3.SeedGenerator.Contracts;
using TrackerCouncil.Smz3.SeedGenerator.Infrastructure;
using TrackerCouncil.Smz3.Shared;
using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.Tracking.Services;

namespace TrackerCouncil.Smz3.Tracking.VoiceCommands;

/// <summary>
/// Provides voice commands that reveal about items and locations.
/// </summary>
public class SpoilerModule : TrackerModule, IOptionalModule
{
    /// <summary>
    /// Specifies the substrings that indicate a Super Metroid location name
    /// is a pretty worthless descriptor.
    /// </summary>
    private static readonly string[] s_worthlessLocationNameIndicators = new[] {
        "behind", "top", "bottom", "middle"
    };

    private readonly Dictionary<ItemType, int> _itemHintsGiven = new();
    private readonly Dictionary<LocationId, int> _locationHintsGiven = new();
    private Playthrough? _playthrough;
    private readonly IRandomizerConfigService _randomizerConfigService;
    private readonly bool _isMultiworld;
    private readonly IGameHintService _gameHintService;
    private readonly PlaythroughService _playthroughService;
    private readonly IMetadataService _metadataService;

    /// <summary>
    /// Initializes a new instance of the <see cref="SpoilerModule"/> class.
    /// </summary>
    /// <param name="tracker">The tracker instance.</param>
    /// <param name="playerProgressionService">Service to get item information</param>
    /// <param name="worldQueryService">Service to get world information</param>
    /// <param name="logger">Used to write logging information.</param>
    /// <param name="randomizerConfigService">Service for retrieving the randomizer config for the world</param>
    /// <param name="gameHintService">Service for getting hints for how important locations are</param>
    /// <param name="playthroughService"></param>
    /// <param name="metadataService"></param>
    public SpoilerModule(TrackerBase tracker, IPlayerProgressionService playerProgressionService, ILogger<SpoilerModule> logger, IWorldQueryService worldQueryService, IRandomizerConfigService randomizerConfigService, IGameHintService gameHintService, PlaythroughService playthroughService, IMetadataService metadataService)
        : base(tracker, playerProgressionService, worldQueryService, logger)
    {
        TrackerBase.HintsEnabled = tracker.World.Config is { Race: false, DisableTrackerHints: false } && tracker.Options.HintsEnabled;
        TrackerBase.SpoilersEnabled = tracker.World.Config is { Race: false, DisableTrackerSpoilers: false } && tracker.Options.SpoilersEnabled;
        _randomizerConfigService = randomizerConfigService;
        _gameHintService = gameHintService;
        _playthroughService = playthroughService;
        _metadataService = metadataService;
        _isMultiworld = tracker.World.Config.MultiWorld;
    }

    private Config Config => _randomizerConfigService.Config;

    /// <summary>
    /// Gives a hint about where to go next.
    /// </summary>
    private void GiveProgressionHint()
    {
        if (TrackerBase is { HintsEnabled: false, SpoilersEnabled: false })
        {
            TrackerBase.Say(x => x.Hints.PromptEnableItemHints);
            return;
        }

        var locations = WorldQueryService.Locations(keysanityByRegion: true).ToList();

        var result = _gameHintService.FindMostValueableLocation(WorldQueryService.Worlds, locations);

        if (result is not { Usefulness: LocationUsefulness.Mandatory })
        {
            TrackerBase.Say(x => x.Hints.NoApplicableHints);
            return;
        }

        TrackerBase.Say(x => x.Hints.AreaSuggestion, args: [result.Value.Location.Region.GetName()]);
    }

    /// <summary>
    /// Gives a hint or spoiler about useful items in an area.
    /// </summary>
    /// <param name="area">The area to give hints about.</param>
    private void GiveAreaHint(IHasLocations area)
    {
        if (TrackerBase is { HintsEnabled: false, SpoilersEnabled: false })
        {
            TrackerBase.Say(x => x.Hints.PromptEnableItemHints);
            return;
        }

        var locations = area.Locations
            .Where(x => x.Cleared == false)
            .ToImmutableList();
        if (locations.Count == 0)
        {
            TrackerBase.Say(x => x.Hints.AreaAlreadyCleared, args: [area.GetName()]);
            return;
        }

        if (TrackerBase.SpoilersEnabled)
        {
            var items = locations
                .Select(x => x.Item)
                .NonNull();
            var itemNames = NaturalLanguage.Join(items, TrackerBase.World.Config);
            TrackerBase.Say(x => x.Spoilers.ItemsInArea, args: [area.GetName(), itemNames]);
        }
        else if (TrackerBase.HintsEnabled)
        {
            if (area.Region!.Config.RomGenerator != RomGenerator.Cas)
            {
                var config = area.Region.Config;
                var anyPossibleProgression = area.Locations.Any(x =>
                    x.ItemType.IsPossibleProgression(config.ZeldaKeysanity, config.MetroidKeysanity, x.Item.IsLocalPlayerItem));
                if (anyPossibleProgression)
                {
                    TrackerBase.Say(x => x.Hints.AreaHasNonCasPossibleProgression, args: [area.GetName()]);
                }
                else if (area is IHasReward region && region.RewardType.IsInAnyCategory(RewardCategory.Crystal, RewardCategory.Metroid))
                {
                    TrackerBase.Say(x => x.Hints.AreaHasNonCasJunkAndReward, args: [area.GetName()]);
                }
                else
                {
                    TrackerBase.Say(x => x.Hints.AreaHasJunk, args: [area.GetName()]);
                }

                return;
            }

            var areaReward = GetRewardForHint(area);
            var usefulness = _gameHintService.GetUsefulness(locations.ToList(), WorldQueryService.Worlds, areaReward);

            if (usefulness == LocationUsefulness.Mandatory)
            {
                TrackerBase.Say(x => x.Hints.AreaHasSomethingMandatory, args: [area.GetName()]);
            }
            else if (usefulness is LocationUsefulness.NiceToHave or LocationUsefulness.Sword)
            {
                TrackerBase.Say(x => x.Hints.AreaHasSomethingGood, args: [area.GetName()]);
            }
            else if (area is IHasReward region)
            {
                if (region.RewardType is RewardType.CrystalBlue or RewardType.CrystalRed)
                {
                    TrackerBase.Say(x => x.Hints.AreaHasJunkAndCrystal, args: [area.GetName()]);
                }
                else
                {
                    TrackerBase.Say(x => x.Hints.AreaHasJunk, args: [area.GetName()]);
                }
            }
            else
            {
                TrackerBase.Say(x => x.Hints.AreaHasJunk, args: [area.GetName()]);
            }
        }
    }

    /// <summary>
    /// Gives a hint or spoiler about the location of an item.
    /// </summary>
    /// <param name="item">The item to find.</param>
    private void RevealItemLocation(Item item)
    {
        if (item.Metadata.HasStages && item.TrackingState >= item.Metadata.MaxStage)
        {
            TrackerBase.Say(x => x.Spoilers.TrackedAllItemsAlready, args: [item.Name]);
            return;
        }
        else if (!item.Metadata.Multiple && item.TrackingState > 0)
        {
            TrackerBase.Say(x => x.Spoilers.TrackedItemAlready, args: [item.Metadata.NameWithArticle]);
            return;
        }

        var markedLocation = WorldQueryService.MarkedLocations()
            .Where(x => x.MarkedItem == item.Type && !x.Cleared)
            .Random();
        if (markedLocation != null)
        {
            var locationName = markedLocation.Metadata.Name;
            var regionName = markedLocation.Region.Metadata.Name;
            TrackerBase.Say(x => x.Spoilers.MarkedItem, args: [item.Metadata.NameWithArticle, locationName, regionName]);
            return;
        }

        // Now that we're ready to give hints, make sure they're turned on
        if (TrackerBase is { HintsEnabled: false, SpoilersEnabled: false })
        {
            TrackerBase.Say(x => x.Hints.PromptEnableItemHints);
            return;
        }

        // Once we're done being a smartass, see if the item can be found at all
        var locations = WorldQueryService.Locations(unclearedOnly: false, outOfLogic: true, itemFilter: item.Type, checkAllWorlds: true)
            .ToImmutableList();
        if (locations.Count == 0)
        {
            if (item.Metadata.Multiple || item.Metadata.HasStages)
                TrackerBase.Say(x => x.Spoilers.ItemsNotFound, args: [item.Metadata.Plural]);
            else
                TrackerBase.Say(x => x.Spoilers.ItemNotFound, args: [item.Metadata.NameWithArticle]);
            return;
        }

        // The item exists, but all locations are cleared
        else if (locations.Count > 0 && locations.All(x => x.Cleared))
        {
            // Prioritize locations that haven't been auto tracked
            var nonAutoTrackedLocations = locations.Where(x => !x.Autotracked);
            var locationsToAnnounce = (nonAutoTrackedLocations.Any() ? nonAutoTrackedLocations : locations).ToList();

            if (locationsToAnnounce.Count == 1)
            {
                TrackerBase.Say(x => x.Spoilers.ItemLocationCleared, args: [item.Metadata.NameWithArticle, locationsToAnnounce.First().RandomName]);
            }
            else
            {
                TrackerBase.Say(x => x.Spoilers.ItemLocationsCleared, args: [item.Metadata.NameWithArticle, NaturalLanguage.Join(locationsToAnnounce)]);
            }

            return;
        }

        // Give hints first (if enabled)
        if (TrackerBase.HintsEnabled && GiveItemLocationHint(item))
            return;

        // Fall back to spoilers if enabled, or prompt to turn them on if hints fail
        if (TrackerBase.SpoilersEnabled)
        {
            if (GiveItemLocationSpoiler(item))
                return;
        }
        else
        {
            TrackerBase.Say(x => x.Spoilers.PromptEnableItemSpoilers);
            return;
        }

        // We ran out of hints?
        Logger.LogWarning("Ran out of hints for {Item}", item.Name[0]);
        TrackerBase.Say(x => x.Hints.NoApplicableHints);
    }

    /// <summary>
    /// Gives a hint or spoiler about the given location.
    /// </summary>
    /// <param name="location">The location to ask about.</param>
    private void RevealLocationItem(Location location)
    {
        var locationName = location.Metadata.Name;
        if (location.Cleared)
        {
            if (TrackerBase.HintsEnabled || TrackerBase.SpoilersEnabled)
            {
                var itemName = _metadataService.GetName(location.Item.Type);
                TrackerBase.Say(x => x.Hints.LocationAlreadyClearedSpoiler, args: [locationName, itemName]);
                return;
            }
            else
            {
                TrackerBase.Say(x => x.Hints.LocationAlreadyCleared, args: [locationName]);
                return;
            }
        }

        if (location.MarkedItem != null)
        {
            var markedItem = WorldQueryService.FirstOrDefault(location.MarkedItem.Value);
            if (markedItem != null)
            {
                TrackerBase.Say(x => x.Spoilers.MarkedLocation, args: [locationName, markedItem.Metadata.NameWithArticle]);
                return;
            }
        }

        if (TrackerBase.HintsEnabled && GiveLocationHints(location))
            return;

        if (TrackerBase.SpoilersEnabled && GiveLocationSpoiler(location))
            return;

        if (!TrackerBase.HintsEnabled)
        {
            TrackerBase.Say(x => x.Hints.PromptEnableLocationHints);
            return;
        }

        if (!TrackerBase.SpoilersEnabled)
        {
            TrackerBase.Say(x => x.Spoilers.PromptEnableLocationSpoilers);
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
    protected virtual bool GiveItemHint(Func<HintsConfig, SchrodingersString?> selectHint,
        Item item, params object?[] additionalArgs)
    {
        var args = Args.Combine(item.Metadata.NameWithArticle, additionalArgs);
        if (!TrackerBase.Say(responses => selectHint(responses.Hints), args: args))
            return false;

        if (_itemHintsGiven.ContainsKey(item.Type))
            _itemHintsGiven[item.Type]++;
        else
            _itemHintsGiven[item.Type] = 1;
        return true;
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
    protected virtual bool GiveItemHint(SchrodingersString? hint,
        Item item, params object?[] additionalArgs)
    {
        var args = Args.Combine(item.Metadata.NameWithArticle, additionalArgs);
        if (!TrackerBase.Say(response: hint, args: args))
            return false;

        if (_itemHintsGiven.ContainsKey(item.Type))
            _itemHintsGiven[item.Type]++;
        else
            _itemHintsGiven[item.Type] = 1;
        return true;
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
    protected virtual bool GiveLocationHint(Func<HintsConfig, SchrodingersString?> selectHint,
        Location location, params object?[] additionalArgs)
    {
        var name = location.Metadata.Name;
        var args = Args.Combine(name, additionalArgs);
        if (!TrackerBase.Say(responses => selectHint(responses.Hints), args: args))
            return false;

        if (_locationHintsGiven.ContainsKey(location.Id))
            _locationHintsGiven[location.Id]++;
        else
            _locationHintsGiven[location.Id] = 1;
        return true;
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
    protected virtual bool GiveLocationHint(SchrodingersString? hint,
        Location location, params object?[] additionalArgs)
    {
        var name = location.Metadata.Name;
        var args = Args.Combine(name, additionalArgs);
        if (!TrackerBase.Say(response: hint, args: args))
            return false;

        if (_locationHintsGiven.ContainsKey(location.Id))
            _locationHintsGiven[location.Id]++;
        else
            _locationHintsGiven[location.Id] = 1;
        return true;
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

    private bool GiveLocationHints(Location location)
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

            return false;
        }

        switch (HintsGiven(location))
        {
            // Who's it for and is it any good?
            case 0:

                var areaReward = GetRewardForHint(location.Region);
                var usefulness = _gameHintService.GetUsefulness([ location ], WorldQueryService.Worlds, areaReward);

                var characterName = location.Item.Type.IsInCategory(ItemCategory.Metroid)
                    ? TrackerBase.CorrectPronunciation(location.World.Config.SamusName)
                    : TrackerBase.CorrectPronunciation(location.World.Config.LinkName);

                switch (usefulness)
                {
                    case LocationUsefulness.Mandatory:
                        return GiveLocationHint(x => x.LocationHasMandatoryItem, location, characterName);
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
                var bookOfMudoraName = _metadataService.GetName(ItemType.Book);
                return GiveLocationHint(x => x.BookHint, location, pedText, bookOfMudoraName);
        }

        return false;
    }

    private bool GiveLocationSpoiler(Location location)
    {
        var locationName = location.Metadata.Name;
        if (location.Item.Type == ItemType.Nothing)
        {
            TrackerBase.Say(x => x.Spoilers.EmptyLocation, args: [locationName]);
            return true;
        }

        if (location.World.Config.RomGenerator != RomGenerator.Cas)
        {
            if (location.Item.Type is ItemType.OtherGameItem or ItemType.OtherGameProgressionItem)
            {
                TrackerBase.Say(x => x.Spoilers.LocationHasOtherGameItem,
                    args: [
                        locationName,
                        location.Item.OriginalName,
                        location.Item.PlayerName
                    ]);
            }
            else
            {
                TrackerBase.Say(x => location.Item.IsLocalPlayerItem ? x.Spoilers.LocationHasItemOwnWorld : x.Spoilers.LocationHasItemOtherWorld,
                    args: [
                        locationName,
                        location.Item.Metadata.NameWithArticle,
                        location.Item.PlayerName
                    ]);
            }

            return true;
        }

        var item = location.Item;
        if (item.Type != ItemType.Nothing)
        {
            if (_isMultiworld)
            {
                TrackerBase.Say(x => location.Item.World == WorldQueryService.World ? x.Spoilers.LocationHasItemOwnWorld : x.Spoilers.LocationHasItemOtherWorld,
                    args: [
                        locationName,
                        item.Metadata.NameWithArticle,
                        location.Item.World.Player
                    ]);
            }
            else
            {
                TrackerBase.Say(x => x.Spoilers.LocationHasItem, args: [locationName, item.Metadata.NameWithArticle]);
            }

            return true;
        }
        else
        {
            TrackerBase.Say(x => x.Spoilers.LocationHasUnknownItem, args: [locationName]);
            return true;
        }
    }

    private bool GiveItemLocationSpoiler(Item item)
    {
        if (item.Metadata == null)
            throw new InvalidOperationException($"No metadata for item '{item.Name}'");

        var reachableLocation = WorldQueryService.Locations(itemFilter: item.Type, keysanityByRegion: true, checkAllWorlds: true)
            .Random();
        if (reachableLocation != null)
        {
            var locationName = reachableLocation.Metadata.Name;
            var regionName = reachableLocation.Region.Metadata.Name;

            if (_isMultiworld)
            {
                if (item.Metadata.Multiple || item.Metadata.HasStages)
                {
                    TrackerBase.Say(x => reachableLocation.World == WorldQueryService.World ? x.Spoilers.ItemsAreAtLocationOwnWorld : x.Spoilers.ItemsAreAtLocationOtherWorld,
                        args: [
                            item.Metadata.NameWithArticle,
                            locationName,
                            regionName,
                            reachableLocation.World.Player
                        ]);
                }
                else
                {
                    TrackerBase.Say(x => reachableLocation.World == WorldQueryService.World ? x.Spoilers.ItemIsAtLocationOwnWorld : x.Spoilers.ItemIsAtLocationOtherWorld,
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
                    TrackerBase.Say(x => x.Spoilers.ItemsAreAtLocation, args: [item.Metadata.NameWithArticle, locationName, regionName]);
                else
                    TrackerBase.Say(x => x.Spoilers.ItemIsAtLocation, args: [item.Metadata.NameWithArticle, locationName, regionName]);
            }

            return true;
        }

        var worldLocation = WorldQueryService.Locations(outOfLogic: true, itemFilter: item.Type, checkAllWorlds: true)
            .Random();
        if (worldLocation != null)
        {
            var locationName = worldLocation.Metadata.Name;
            var regionName = worldLocation.Region.Metadata.Name;

            if (_isMultiworld)
            {
                if (item.Metadata.Multiple || item.Metadata.HasStages)
                {
                    TrackerBase.Say(x => worldLocation.World == WorldQueryService.World ? x.Spoilers.ItemsAreAtOutOfLogicLocationOwnWorld : x.Spoilers.ItemsAreAtOutOfLogicLocationOtherWorld,
                        args: [
                            item.Metadata.NameWithArticle,
                            locationName,
                            regionName,
                            worldLocation.World.Player
                        ]);
                }
                else
                {
                    TrackerBase.Say(x => worldLocation.World == WorldQueryService.World ? x.Spoilers.ItemIsAtOutOfLogicLocationOwnWorld : x.Spoilers.ItemIsAtOutOfLogicLocationOtherWorld,
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
                    TrackerBase.Say(x => x.Spoilers.ItemsAreAtOutOfLogicLocation, args: [item.Metadata.NameWithArticle, locationName, regionName]);
                else
                    TrackerBase.Say(x => x.Spoilers.ItemIsAtOutOfLogicLocation, args: [item.Metadata.NameWithArticle, locationName, regionName]);
            }

            return true;
        }

        return false;
    }

    private bool GiveItemLocationHint(Item item)
    {
        var itemLocations = WorldQueryService.Locations(outOfLogic: true, itemFilter: item.Type, checkAllWorlds: true).ToList();

        if (!itemLocations.Any())
        {
            Logger.LogInformation("Can't find any uncleared locations with {Item}", item.Type.GetDescription());
            return false;
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

                    var isInLogic = itemLocations.Any(x => x.IsRelevant(PlayerProgressionService.GetProgression(x.Region)) && x.World.IsLocalWorld);
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
                    if (itemLocations.All(x => !x.IsRelevant(PlayerProgressionService.GetProgression(x.Region))))
                    {
                        var randomLocation = itemLocations.Where(x => !x.IsRelevant(PlayerProgressionService.GetProgression(x.Region))).Random();

                        if (randomLocation == null)
                        {
                            return GiveItemHint(x => x.NoApplicableHints, item);
                        }

                        if (!randomLocation.World.IsLocalWorld)
                        {
                            return GiveItemHint(x => x.ItemInPlayerWorld, item,
                                randomLocation.World.Config.PhoneticName);
                        }

                        var progression = PlayerProgressionService.GetProgression(randomLocation.Region);
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
                                .Select(x => WorldQueryService.FirstOrDefault(x))
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
                    Logger.LogDebug("{Item} spoiler: sphere {Sphere}, early: {Early}, late: {Late}", item, sphere, earlyThreshold, lateThreshold);
                    if (sphere == 0)
                        return GiveItemHint(x => x.ItemInSphereZero, item);
                    if (sphere <= earlyThreshold)
                        return GiveItemHint(x => x.ItemInEarlySphere, item);
                    if (sphere >= lateThreshold)
                        return GiveItemHint(x => x.ItemInLateSphere, item);

                    var areaWithoutItem = TrackerBase.World.Regions
                        .GroupBy(x => x.Area)
                        .Where(x => x.SelectMany(r => r.Locations)
                            .Where(l => l.Cleared == false)
                            .All(l => l.Item.Type != item.Type))
                        .Select(x => x.Key)
                        .Random();
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
                                TrackerBase.Say(x => x.Hints.ItemInPlayerWorldRegionRoomPrefixHint,
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
                                    TrackerBase.Say(x => x.Hints.ItemInPlayerWorldRegionRoomPrefixHint,
                                        args: [randomLocation.World.Config.PhoneticName]);
                                return GiveItemHint(roomHint, item);
                            }
                        }

                        var locationHint = randomLocation.Metadata.Hints;
                        if (locationHint is { Count: > 0 })
                        {
                            if (!randomLocation.World.IsLocalWorld)
                                TrackerBase.Say(x => x.Hints.ItemInPlayerWorldRegionRoomPrefixHint,
                                    args: [randomLocation.World.Config.PhoneticName]);
                            return GiveItemHint(locationHint, item);
                        }
                    }

                    randomLocation = GetRandomItemLocationWithFilter(item, x => x.VanillaItem.IsInCategory(ItemCategory.Plentiful));
                    if (randomLocation != null && randomLocation.VanillaItem.IsInCategory(ItemCategory.Plentiful))
                    {
                        if (!randomLocation.World.IsLocalWorld)
                            TrackerBase.Say(x => x.Hints.ItemInPlayerWorldRegionRoomPrefixHint,
                                args: [randomLocation.World.Config.PhoneticName]);

                        if (randomLocation.Region is SMRegion
                            && randomLocation.Name.ContainsAny(StringComparison.OrdinalIgnoreCase, s_worthlessLocationNameIndicators))
                        {
                            // Just give the name of the location from the
                            // original SMZ3 randomizer code, it's vague
                            // enough
                            return GiveItemHint(x => x.ItemHasBadVanillaLocationName, item, randomLocation.Name);
                        }

                        var vanillaItem = WorldQueryService.FirstOrDefault(randomLocation.VanillaItem);
                        return GiveItemHint(x => x.ItemIsInVanillaJunkLocation, item, vanillaItem?.Metadata.Name ?? randomLocation.VanillaItem.GetDescription());
                    }

                    // If there isn't any location with this item that has a
                    // hint, let it fall through so tracker can tell the
                    // player to enable spoilers
                    Logger.LogInformation("No more hints for {Item}", item);
                }
                break;
        }

        return false;
    }

    private int HintsGiven(Item item) => _itemHintsGiven.GetValueOrDefault(item.Type, 0);

    private int HintsGiven(Location location) => _locationHintsGiven.GetValueOrDefault(location.Id, 0);

    private Location? GetRandomItemLocationWithFilter(Item item, Func<Location, bool> predicate)
    {
        var randomLocation = WorldQueryService.Locations(itemFilter: item.Type, keysanityByRegion: true, checkAllWorlds: true)
            .Where(predicate)
            .Random();

        if (randomLocation == null)
        {
            // If the item is not at any accessible location, try to look in
            // out-of-logic places, too.
            randomLocation = WorldQueryService.Locations(outOfLogic: true,  itemFilter: item.Type, keysanityByRegion: true, checkAllWorlds: true)
                .Where(predicate)
                .Random();
        }

        return randomLocation;
    }

    [SupportedOSPlatform("windows")]
    private GrammarBuilder GetItemSpoilerRule()
    {
        var items = GetItemNames();

        return new GrammarBuilder()
            .Append("Hey tracker, ")
            .OneOf("where is", "where's", "where are", "where can I find",
                "where the fuck is", "where the hell is",
                "where the heck is")
            .Optional("the", "a", "an")
            .Append(ItemNameKey, items);
    }

    [SupportedOSPlatform("windows")]
    private GrammarBuilder GetLocationSpoilerRule()
    {
        var locations = GetLocationNames();

        var whatsAtRule = new GrammarBuilder()
            .Append("Hey tracker, ")
            .OneOf("what's", "what is")
            .OneOf("at", "in")
            .Optional("the")
            .Append(LocationKey, locations);

        var whatDoesLocationHaveRule = new GrammarBuilder()
            .Append("Hey tracker, ")
            .Append("what does")
            .Optional("the")
            .Append(LocationKey, locations)
            .Append("have")
            .Optional("for me");

        return GrammarBuilder.Combine(whatsAtRule, whatDoesLocationHaveRule);
    }

    [SupportedOSPlatform("windows")]
    private GrammarBuilder GetEnableSpoilersRule()
    {
        return new GrammarBuilder()
            .Append("Hey tracker, ")
            .OneOf("enable", "turn on")
            .Append("spoilers");
    }

    [SupportedOSPlatform("windows")]
    private GrammarBuilder GetDisableSpoilersRule()
    {
        return new GrammarBuilder()
            .Append("Hey tracker, ")
            .OneOf("disable", "turn off")
            .Append("spoilers");
    }

    [SupportedOSPlatform("windows")]
    private GrammarBuilder GetEnableHintsRule()
    {
        return new GrammarBuilder()
            .Append("Hey tracker, ")
            .OneOf("enable", "turn on")
            .Append("hints");
    }

    [SupportedOSPlatform("windows")]
    private GrammarBuilder GetDisableHintsRule()
    {
        return new GrammarBuilder()
            .Append("Hey tracker, ")
            .OneOf("disable", "turn off")
            .Append("hints");
    }

    [SupportedOSPlatform("windows")]
    private GrammarBuilder GetProgressionHintRule()
    {
        return new GrammarBuilder()
            .Append("Hey tracker, ")
            .OneOf("give me a hint",
                "give me a suggestion",
                "can you give me a hint?",
                "do you have any suggestions?",
                "where should I go?",
                "what should I do?");
    }

    [SupportedOSPlatform("windows")]
    private GrammarBuilder GetLocationUsefulnessHintRule()
    {
        var regionNames = GetRegionNames();
        var regionGrammar = new GrammarBuilder()
            .Append("Hey tracker, ")
            .OneOf("is there anything useful",
                "is there anything I need",
                "is there anything good",
                "is there anything",
                "what's left",
                "what's")
            .OneOf("in", "at")
            .Append(RegionKey, regionNames);

        var roomNames = GetRoomNames();
        var roomGrammar = new GrammarBuilder()
            .Append("Hey tracker, ")
            .OneOf("is there anything useful",
                "is there anything I need",
                "is there anything good",
                "is there anything",
                "what's left",
                "what's")
            .OneOf("in", "at")
            .Append(RoomKey, roomNames);

        return GrammarBuilder.Combine(regionGrammar, roomGrammar);
    }

    [SupportedOSPlatform("windows")]
    public override void AddCommands()
    {
        if (TrackerBase.World.Config.Race) return;

        _playthroughService.TryGenerate(new[] { TrackerBase.World }, TrackerBase.World.Config, out _playthrough);

        if (!TrackerBase.World.Config.DisableTrackerHints)
        {
            AddCommand("Enable hints", GetEnableHintsRule(), (_) =>
            {
                if (!TrackerBase.HintsEnabled)
                {
                    TrackerBase.HintsEnabled = true;
                    TrackerBase.Say(x => x.Hints.EnabledHints);
                }
                else
                {
                    TrackerBase.Say(x => x.Hints.AlreadyEnabledHints);
                }
            });
            AddCommand("Disable hints", GetDisableHintsRule(), (_) =>
            {
                if (TrackerBase.HintsEnabled)
                {
                    TrackerBase.HintsEnabled = false;
                    TrackerBase.Say(x => x.Hints.DisabledHints);
                }
                else
                {
                    TrackerBase.Say(x => x.Hints.AlreadyDisabledHints);
                }
            });
            AddCommand("Give progression hint", GetProgressionHintRule(), (_) =>
            {
                GiveProgressionHint();
            });

            AddCommand("Give area hint", GetLocationUsefulnessHintRule(), (result) =>
            {
                var area = GetAreaFromResult(TrackerBase, result);
                GiveAreaHint(area);
            });
        }

        if (!TrackerBase.World.Config.DisableTrackerSpoilers)
        {
            AddCommand("Enable spoilers", GetEnableSpoilersRule(), (_) =>
            {
                if (!TrackerBase.SpoilersEnabled)
                {
                    TrackerBase.SpoilersEnabled = true;
                    TrackerBase.Say(x => x.Spoilers.EnabledSpoilers);
                }
                else
                {
                    TrackerBase.Say(x => x.Spoilers.AlreadyEnabledSpoilers);
                }
            });
            AddCommand("Disable spoilers", GetDisableSpoilersRule(), (_) =>
            {
                if (TrackerBase.SpoilersEnabled)
                {
                    TrackerBase.SpoilersEnabled = false;
                    TrackerBase.Say(x => x.Spoilers.DisabledSpoilers);
                }
                else
                {
                    TrackerBase.Say(x => x.Spoilers.AlreadyDisabledSpoilers);
                }
            });

            AddCommand("Reveal item location", GetItemSpoilerRule(), (result) =>
            {
                var item = GetItemFromResult(TrackerBase, result, out _);
                RevealItemLocation(item);
            });

            AddCommand("Reveal location item", GetLocationSpoilerRule(), (result) =>
            {
                var location = GetLocationFromResult(TrackerBase, result);
                RevealLocationItem(location);
            });
        }
    }
}
