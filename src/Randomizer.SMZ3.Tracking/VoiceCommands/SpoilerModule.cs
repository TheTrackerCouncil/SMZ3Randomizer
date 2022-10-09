using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

using Microsoft.Extensions.Logging;
using Randomizer.Data.Logic;
using Randomizer.Data.WorldData.Regions;
using Randomizer.Data.WorldData;
using Randomizer.Shared;
using Randomizer.SMZ3.Text;
using Randomizer.Data.Configuration;
using Randomizer.Data.Configuration.ConfigTypes;
using Randomizer.SMZ3.Tracking.Services;
using Randomizer.Data.Options;

namespace Randomizer.SMZ3.Tracking.VoiceCommands
{
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
        private readonly Dictionary<int, int> _locationHintsGiven = new();
        private readonly Playthrough? _playthrough;
        private readonly IRandomizerConfigService _randomizerConfigService;
        private Config _config => _randomizerConfigService.Config;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpoilerModule"/> class.
        /// </summary>
        /// <param name="tracker">The tracker instance.</param>
        /// <param name="itemService">Service to get item information</param>
        /// <param name="worldService">Service to get world information</param>
        /// <param name="logger">Used to write logging information.</param>
        /// <param name="randomizerConfigService">Service for retrieving the randomizer config for the world</param>
        public SpoilerModule(Tracker tracker, IItemService itemService, ILogger<SpoilerModule> logger, IWorldService worldService, IRandomizerConfigService randomizerConfigService)
            : base(tracker, itemService, worldService, logger)
        {
            Tracker.HintsEnabled = !tracker.World.Config.Race && !tracker.World.Config.DisableTrackerHints && tracker.Options.HintsEnabled;
            Tracker.SpoilersEnabled = !tracker.World.Config.Race && !tracker.World.Config.DisableTrackerSpoilers && tracker.Options.SpoilersEnabled;
            if (tracker.World.Config.Race) return;
            _randomizerConfigService = randomizerConfigService;

            Playthrough.TryGenerate(new[] { tracker.World }, tracker.World.Config, out _playthrough);

            if (!tracker.World.Config.DisableTrackerHints)
            {
                AddCommand("Enable hints", GetEnableHintsRule(), (tracker, result) =>
                {
                    Tracker.HintsEnabled = true;
                    tracker.Say(x => x.Hints.EnabledHints);
                });
                AddCommand("Disable hints", GetDisableHintsRule(), (tracker, result) =>
                {
                    Tracker.HintsEnabled = false;
                    tracker.Say(x => x.Hints.DisabledHints);
                });
                AddCommand("Give progression hint", GetProgressionHintRule(), (tracker, result) =>
                {
                    GiveProgressionHint();
                });

                AddCommand("Give area hint", GetLocationUsefulnessHintRule(), (tracker, result) =>
                {
                    var area = GetAreaFromResult(tracker, result);
                    GiveAreaHint(area);
                });
            }

            if (!tracker.World.Config.DisableTrackerSpoilers)
            {
                AddCommand("Enable spoilers", GetEnableSpoilersRule(), (tracker, result) =>
                {
                    Tracker.SpoilersEnabled = true;
                    tracker.Say(x => x.Spoilers.EnabledSpoilers);
                });
                AddCommand("Disable spoilers", GetDisableSpoilersRule(), (tracker, result) =>
                {
                    Tracker.SpoilersEnabled = false;
                    tracker.Say(x => x.Spoilers.DisabledSpoilers);
                });

                AddCommand("Reveal item location", GetItemSpoilerRule(), (tracker, result) =>
                {
                    var item = GetItemFromResult(tracker, result, out var itemName);
                    RevealItemLocation(item);
                });

                AddCommand("Reveal location item", GetLocationSpoilerRule(), (tracker, result) =>
                {
                    var location = GetLocationFromResult(tracker, result);
                    RevealLocationItem(location);
                });
            }
        }

        

        /// <summary>
        /// Gives a hint about where to go next.
        /// </summary>
        public void GiveProgressionHint()
        {
            if (!Tracker.HintsEnabled && !Tracker.SpoilersEnabled)
            {
                Tracker.Say(x => x.Hints.PromptEnableItemHints);
                return;
            }

            var locationWithProgressionItem = WorldService.Locations(keysanityByRegion: true)
                .Where(x => x.Item.State.TrackingState == 0 && x.Item.Metadata.IsProgression(_config) == true)
                .Random();

            if (locationWithProgressionItem != null)
            {
                Tracker.Say(x => x.Hints.AreaSuggestion, locationWithProgressionItem.Region.GetName());
                return;
            }

            Tracker.Say(x => x.Hints.NoApplicableHints);
        }

        /// <summary>
        /// Gives a hint or spoiler about useful items in an area.
        /// </summary>
        /// <param name="area">The area to give hints about.</param>
        public void GiveAreaHint(IHasLocations area)
        {
            if (!Tracker.HintsEnabled && !Tracker.SpoilersEnabled)
            {
                Tracker.Say(x => x.Hints.PromptEnableItemHints);
                return;
            }

            var locations = area.Locations
                .Where(x => !x.State.Cleared)
                .ToImmutableList();
            if (locations.Count == 0)
            {
                Tracker.Say(x => x.Hints.AreaAlreadyCleared, area.GetName());
                return;
            }

            var items = locations
                .Select(x => x.Item)
                .NonNull();
            if (Tracker.SpoilersEnabled)
            {
                var itemNames = NaturalLanguage.Join(items, Tracker.World.Config);
                Tracker.Say(x => x.Spoilers.ItemsInArea, area.GetName(), itemNames);
            }
            else if (Tracker.HintsEnabled)
            {
                if (items.Any(x => !x.Metadata.IsJunk(Tracker.World.Config)))
                {
                    Tracker.Say(x => x.Hints.AreaHasSomethingGood, area.GetName());
                }
                else if (area is IHasReward region)
                {
                    if (region.RewardType == RewardType.CrystalBlue
                        || region.RewardType == RewardType.CrystalRed)
                    {
                        Tracker.Say(x => x.Hints.AreaHasJunkAndCrystal, area.GetName());
                    }
                    else if (Tracker.IsWorth(region.RewardType))
                    {
                        Tracker.Say(x => x.Hints.AreaWorthComplicated, area.GetName());
                    }
                    else
                    {
                        Tracker.Say(x => x.Hints.AreaHasJunk, area.GetName());
                    }
                }
                else
                {
                    Tracker.Say(x => x.Hints.AreaHasJunk, area.GetName());
                }
            }
        }

        /// <summary>
        /// Gives a hint or spoiler about the location of an item.
        /// </summary>
        /// <param name="item">The item to find.</param>
        public void RevealItemLocation(Item item)
        {
            if (item.Metadata.HasStages && item.State.TrackingState >= item.Metadata.MaxStage)
            {
                Tracker.Say(x => x.Spoilers.TrackedAllItemsAlready, item.Name);
                return;
            }
            else if (!item.Metadata.Multiple && item.State.TrackingState > 0)
            {
                Tracker.Say(x => x.Spoilers.TrackedItemAlready, item.Metadata.NameWithArticle);
                return;
            }

            var markedLocation = WorldService.MarkedLocations()
                .Where(x => x.State.MarkedItem == item.Type && !x.State.Cleared)
                .Random();
            if (markedLocation != null)
            {
                var locationName = markedLocation.Metadata.Name;
                var regionName = markedLocation.Region.Metadata.Name;
                Tracker.Say(x => x.Spoilers.MarkedItem, item.Metadata.NameWithArticle, locationName, regionName);
                return;
            }

            // Now that we're ready to give hints, make sure they're turned on
            if (!Tracker.HintsEnabled && !Tracker.SpoilersEnabled)
            {
                Tracker.Say(x => x.Hints.PromptEnableItemHints);
                return;
            }

            // Once we're done being a smartass, see if the item can be found at all
            var locations = WorldService.Locations(unclearedOnly: false, outOfLogic: true, itemFilter: item.Type)
                .ToImmutableList();
            if (locations.Count == 0)
            {
                if (item.Metadata.Multiple || item.Metadata.HasStages)
                    Tracker.Say(x => x.Spoilers.ItemsNotFound, item.Metadata.Plural);
                else
                    Tracker.Say(x => x.Spoilers.ItemNotFound, item.Metadata.NameWithArticle);
                return;
            }
            else if (locations.Count > 0 && locations.All(x => x.State.Cleared))
            {
                // The item exists, but all locations are cleared
                Tracker.Say(x => x.Spoilers.LocationsCleared, item.Metadata.NameWithArticle);
                return;
            }

            // Give hints first (if enabled)
            if (Tracker.HintsEnabled && GiveItemLocationHint(item))
                return;

            // Fall back to spoilers if enabled, or prompt to turn them on if hints fail
            if (Tracker.SpoilersEnabled)
            {
                if (GiveItemLocationSpoiler(item))
                    return;
            }
            else
            {
                Tracker.Say(x => x.Spoilers.PromptEnableItemSpoilers);
                return;
            }

            // We ran out of hints?
            Logger.LogWarning("Ran out of hints for {Item}", item.Name[0]);
            Tracker.Say(x => x.Hints.NoApplicableHints);
        }

        /// <summary>
        /// Gives a hint or spoiler about the given location.
        /// </summary>
        /// <param name="location">The location to ask about.</param>
        public void RevealLocationItem(Location location)
        {
            var locationName = location.Metadata.Name;
            if (location.State.Cleared)
            {
                if (Tracker.HintsEnabled || Tracker.SpoilersEnabled)
                {
                    var itemName = ItemService.GetName(location.Item.Type);
                    Tracker.Say(x => x.Hints.LocationAlreadyClearedSpoiler, locationName, itemName);
                    return;
                }
                else
                {
                    Tracker.Say(x => x.Hints.LocationAlreadyCleared, locationName);
                    return;
                }
            }

            if (location.State.MarkedItem != null)
            {
                var markedItem = ItemService.FirstOrDefault(location.State.MarkedItem.Value);
                if (markedItem != null)
                {
                    Tracker.Say(x => x.Spoilers.MarkedLocation, locationName, markedItem.Metadata.NameWithArticle);
                    return;
                }
            }

            if (Tracker.HintsEnabled && GiveLocationHints(location))
                return;

            if (Tracker.SpoilersEnabled && GiveLocationSpoiler(location))
                return;

            if (!Tracker.HintsEnabled)
            {
                Tracker.Say(x => x.Hints.PromptEnableLocationHints);
                return;
            }

            if (!Tracker.SpoilersEnabled)
            {
                Tracker.Say(x => x.Spoilers.PromptEnableLocationSpoilers);
                return;
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
            if (!Tracker.Say(responses => selectHint(responses.Hints), args))
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
            if (!Tracker.Say(hint, args))
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
            if (!Tracker.Say(responses => selectHint(responses.Hints), args))
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
            if (!Tracker.Say(hint, args))
                return false;

            if (_locationHintsGiven.ContainsKey(location.Id))
                _locationHintsGiven[location.Id]++;
            else
                _locationHintsGiven[location.Id] = 1;
            return true;
        }

        private bool GiveLocationHints(Location location)
        {
            switch (HintsGiven(location))
            {
                // Who's it for and is it any good?
                case 0:
                    var characterName = location.Item.Type.IsInCategory(ItemCategory.Metroid)
                        ? Tracker.CorrectPronunciation(Tracker.World.Config.SamusName)
                        : Tracker.CorrectPronunciation(Tracker.World.Config.LinkName);

                    if (location.Item.Metadata?.IsJunk(Tracker.World.Config) == true)
                    {
                        return GiveLocationHint(x => x.LocationHasJunkItem, location, characterName);
                    }

                    // Todo: Add check for if it's progression

                    if (location.Item.Metadata?.IsGood(Tracker.World.Config) == true)
                    {
                        return GiveLocationHint(x => x.LocationHasUsefulItem, location, characterName);
                    }

                    return GiveLocationHint(x => x.NoApplicableHints, location);

                // Try to give a hint from the config
                case 1:
                    var hint = location.Item.Metadata?.Hints;
                    if (hint != null && hint.Count > 0)
                        return GiveLocationHint(hint, location);

                    return GiveLocationHint(x => x.NoApplicableHints, location);

                // Consult the Book of Mudora
                case 2:
                    var pedText = Texts.ItemTextbox(location.Item).Replace('\n', ' ');
                    var bookOfMudoraName = ItemService.GetName(ItemType.Book);
                    return GiveLocationHint(x => x.BookHint, location, pedText, bookOfMudoraName);
            }

            return false;
        }

        private bool GiveLocationSpoiler(Location location)
        {
            var locationName = location.Metadata.Name;
            if (location.Item == null || location.Item.Type == ItemType.Nothing)
            {
                Tracker.Say(x => x.Spoilers.EmptyLocation, locationName);
                return true;
            }

            var item = location.Item;
            if (item != null)
            {
                Tracker.Say(x => x.Spoilers.LocationHasItem, locationName, item.Metadata.NameWithArticle);
                return true;
            }
            else
            {
                Tracker.Say(x => x.Spoilers.LocationHasUnknownItem, locationName, location.Item);
                return true;
            }
        }

        private bool GiveItemLocationSpoiler(Item item)
        {
            var reachableLocation = WorldService.Locations(itemFilter: item.Type, keysanityByRegion: true)
                .Random();
            if (reachableLocation != null)
            {
                var locationName = reachableLocation.Metadata.Name;
                var regionName = reachableLocation.Region.Metadata.Name;
                if (item.Metadata.Multiple || item.Metadata.HasStages)
                    Tracker.Say(x => x.Spoilers.ItemsAreAtLocation, item.Metadata.NameWithArticle, locationName, regionName);
                else
                    Tracker.Say(x => x.Spoilers.ItemIsAtLocation, item.Metadata.NameWithArticle, locationName, regionName);
                return true;
            }

            var worldLocation = WorldService.Locations(outOfLogic: true, itemFilter: item.Type)
                .Random();
            if (worldLocation != null)
            {
                var locationName = worldLocation.Metadata.Name;
                var regionName = worldLocation.Region.Metadata.Name;
                if (item.Metadata.Multiple || item.Metadata.HasStages)
                    Tracker.Say(x => x.Spoilers.ItemsAreAtOutOfLogicLocation, item.Metadata.NameWithArticle, locationName, regionName);
                else
                    Tracker.Say(x => x.Spoilers.ItemIsAtOutOfLogicLocation, item.Metadata.NameWithArticle, locationName, regionName);
                return true;
            }

            return false;
        }

        private bool GiveItemLocationHint(Item item)
        {
            var itemLocations = WorldService.Locations(outOfLogic: true, itemFilter: item.Type);

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
                case 0:
                    {
                        var isInLogic = itemLocations.Any(x => x.IsAvailable(ItemService.GetProgression(x.Region)));
                        if (!isInLogic)
                            return GiveItemHint(x => x.ItemNotInLogic, item);

                        var isOnlyInSuperMetroid = itemLocations.Select(x => x.Region).All(x => x is SMRegion);
                        if (isOnlyInSuperMetroid)
                            return GiveItemHint(x => x.ItemInSuperMetroid, item);

                        var isOnlyInALinkToThePast = itemLocations.Select(x => x.Region).All(x => x is Z3Region);
                        if (isOnlyInALinkToThePast)
                            return GiveItemHint(x => x.ItemInALttP, item);

                        return GiveItemHint(x => x.NoApplicableHints, item);
                    }

                // Lv1 hints are still pretty vague:
                // - Is it early or late?
                // - Where will you NOT find it?
                case 1:
                    {
                        if (itemLocations.All(x => !x.IsAvailable(ItemService.GetProgression(x.Region))))
                        {
                            var randomLocation = itemLocations.Where(x => !x.IsAvailable(ItemService.GetProgression(x.Region))).Random();

                            if (randomLocation == null)
                            {
                                return GiveItemHint(x => x.NoApplicableHints, item);
                            }

                            var progression = ItemService.GetProgression(randomLocation.Region);
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
                                    .Select(x => ItemService.FirstOrDefault(x))
                                    .Random();
                                if (randomMissingItem != null)
                                    return GiveItemHint(x => x.ItemRequiresOtherItem, item, randomMissingItem.Metadata.NameWithArticle);
                            }
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

                        var areaWithoutItem = Tracker.World.Regions
                            .GroupBy(x => x.Area)
                            .Where(x => !x.SelectMany(x => x.Locations)
                                          .Where(x => !x.State.Cleared)
                                          .Any(l => l.Item.Type == item.Type))
                            .Select(x => x.Key)
                            .Random();
                        if (areaWithoutItem != null)
                            return GiveItemHint(x => x.ItemNotInArea, item, areaWithoutItem);

                        return GiveItemHint(x => x.NoApplicableHints, item);
                    }

                // Lv2 hints are less vague still: have you there or not?
                // - Is it in a dungeon? Have you been there before or not?
                // - Is it in an area you've been before?
                case 2:
                    {
                        var randomLocation = GetRandomItemLocationWithFilter(item, x => true);
                        if (randomLocation?.Region is Z3Region and IHasReward dungeon && dungeon.RewardType != RewardType.Agahnim)
                        {
                            if (randomLocation.Region.Locations.Any(x => x.State.Cleared))
                                return GiveItemHint(x => x.ItemInPreviouslyVisitedDungeon, item);
                            else
                                return GiveItemHint(x => x.ItemInUnvisitedDungeon, item);
                        }

                        if (randomLocation?.Region.Locations.Any(x => x.State.Cleared) == true)
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
                            if (regionHint != null && regionHint.Count > 0)
                                return GiveItemHint(regionHint, item);
                        }

                        return GiveItemHint(x => x.NoApplicableHints, item);
                    }

                // Lv4 hints give a clue about the room or location itself
                case 4:
                    {
                        var randomLocation = GetRandomItemLocationWithFilter(item,
                            location =>
                            {
                                if (location.Room != null && location.Room.Metadata.Hints?.Count > 0)
                                    return true;
                                return location.Metadata.Hints?.Count > 0;
                            });

                        if (randomLocation != null)
                        {
                            if (randomLocation.Room != null)
                            {
                                var roomHint = randomLocation.Room.Metadata.Hints;
                                if (roomHint != null && roomHint.Count > 0)
                                {
                                    return GiveItemHint(roomHint, item);
                                }
                            }

                            var locationHint = randomLocation.Metadata.Hints;
                            if (locationHint != null && locationHint.Count > 0)
                            {
                                return GiveItemHint(locationHint, item);
                            }
                        }

                        randomLocation = GetRandomItemLocationWithFilter(item, x => x.VanillaItem.IsInCategory(ItemCategory.Plentiful));
                        if (randomLocation != null && randomLocation.VanillaItem.IsInCategory(ItemCategory.Plentiful))
                        {
                            if (randomLocation.Region is SMRegion
                                && randomLocation.Name.ContainsAny(StringComparison.OrdinalIgnoreCase, s_worthlessLocationNameIndicators))
                            {
                                // Just give the name of the location from the
                                // original SMZ3 randomizer code, it's vague
                                // enough
                                return GiveItemHint(x => x.ItemHasBadVanillaLocationName, item, randomLocation.Name);
                            }

                            var vanillaItem = ItemService.FirstOrDefault(randomLocation.VanillaItem);
                            return GiveItemHint(x => x.ItemIsInVanillaJunkLocation, item, vanillaItem?.Metadata?.Name ?? randomLocation.VanillaItem.GetDescription());
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
            var randomLocation = WorldService.Locations(itemFilter: item.Type, keysanityByRegion: true)
                .Where(predicate)
                .Random();

            if (randomLocation == null)
            {
                // If the item is not at any accessible location, try to look in
                // out-of-logic places, too.
                randomLocation = WorldService.Locations(outOfLogic: true,  itemFilter: item.Type, keysanityByRegion: true)
                    .Where(predicate)
                    .Random();
            }

            return randomLocation;
        }

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

        private GrammarBuilder GetEnableSpoilersRule()
        {
            return new GrammarBuilder()
                .Append("Hey tracker, ")
                .OneOf("enable", "turn on")
                .Append("spoilers");
        }

        private GrammarBuilder GetDisableSpoilersRule()
        {
            return new GrammarBuilder()
                .Append("Hey tracker, ")
                .OneOf("disable", "turn off")
                .Append("spoilers");
        }

        private GrammarBuilder GetEnableHintsRule()
        {
            return new GrammarBuilder()
                .Append("Hey tracker, ")
                .OneOf("enable", "turn on")
                .Append("hints");
        }

        private GrammarBuilder GetDisableHintsRule()
        {
            return new GrammarBuilder()
                .Append("Hey tracker, ")
                .OneOf("disable", "turn off")
                .Append("hints");
        }

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
    }
}
