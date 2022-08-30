using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

using Microsoft.Extensions.Logging;

using Randomizer.Shared;
using Randomizer.SMZ3.Regions;
using Randomizer.SMZ3.Text;
using Randomizer.SMZ3.Tracking.Configuration;
using Randomizer.SMZ3.Tracking.Configuration.ConfigTypes;
using Randomizer.SMZ3.Tracking.Services;

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
        private readonly IItemService _itemService;
        private readonly IRandomizerConfigService _randomizerConfigService;
        private Config _config => _randomizerConfigService.Config;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpoilerModule"/> class.
        /// </summary>
        /// <param name="tracker">The tracker instance.</param>
        /// <param name="itemService">Used to manage item information.</param>
        /// <param name="logger">Used to write logging information.</param>
        /// <param name="randomizerConfigService">Service for retrieving the randomizer config for the world</param>
        public SpoilerModule(Tracker tracker, IItemService itemService, ILogger<SpoilerModule> logger, IRandomizerConfigService randomizerConfigService)
            : base(tracker, itemService, logger)
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

            var locationWithProgressionItem = Tracker.World.Locations
                .Where(x => !x.Cleared && x.IsAvailable(Tracker.GetProgression(x.Region)))
                .Where(x => !ItemService.IsTracked(x.Item.Type) && ItemService?.GetOrDefault(x.Item.Type)?.IsProgression(_config) == true)
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
                .Where(x => !x.Cleared)
                .ToImmutableList();
            if (locations.Count == 0)
            {
                Tracker.Say(x => x.Hints.AreaAlreadyCleared, area.GetName());
                return;
            }

            var items = locations
                .Select(x => ItemService.GetOrDefault(x))
                .NonNull();
            if (Tracker.SpoilersEnabled)
            {
                var itemNames = NaturalLanguage.Join(items, Tracker.World.Config);
                Tracker.Say(x => x.Spoilers.ItemsInArea, area.GetName(), itemNames);
            }
            else if (Tracker.HintsEnabled)
            {
                if (items.Any(x => !x.IsJunk(Tracker.World.Config)))
                {
                    Tracker.Say(x => x.Hints.AreaHasSomethingGood, area.GetName());
                }
                else if (area is IHasReward region)
                {
                    if (region.Reward == RewardType.CrystalBlue
                        || region.Reward == RewardType.CrystalRed)
                    {
                        Tracker.Say(x => x.Hints.AreaHasJunkAndCrystal, area.GetName());
                    }
                    else if (Tracker.IsWorth(region.Reward))
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
        public void RevealItemLocation(ItemData item)
        {
            if (item.HasStages && item.TrackingState >= item.MaxStage)
            {
                Tracker.Say(x => x.Spoilers.TrackedAllItemsAlready, item.Name);
                return;
            }
            else if (!item.Multiple && item.TrackingState > 0)
            {
                Tracker.Say(x => x.Spoilers.TrackedItemAlready, item.NameWithArticle);
                return;
            }

            var markedLocation = Tracker.MarkedLocations
                .Where(x => x.Value.InternalItemType == item.InternalItemType)
                .Select(x => Tracker.World.Locations.Single(y => y.Id == x.Key))
                .Where(x => !x.Cleared)
                .Random();
            if (markedLocation != null)
            {
                var locationName = Tracker.GetName(markedLocation);
                var regionName = Tracker.WorldInfo.Region(markedLocation.Region).Name;
                Tracker.Say(x => x.Spoilers.MarkedItem, item.NameWithArticle, locationName, regionName);
                return;
            }

            // Now that we're ready to give hints, make sure they're turned on
            if (!Tracker.HintsEnabled && !Tracker.SpoilersEnabled)
            {
                Tracker.Say(x => x.Hints.PromptEnableItemHints);
                return;
            }

            // Once we're done being a smartass, see if the item can be found at all
            var locations = Tracker.World.Locations
                .Where(x => x.Item.Type == item.InternalItemType)
                .ToImmutableList();
            if (locations.Count == 0)
            {
                if (item.Multiple || item.HasStages)
                    Tracker.Say(x => x.Spoilers.ItemsNotFound, item.Plural);
                else
                    Tracker.Say(x => x.Spoilers.ItemNotFound, item.NameWithArticle);
                return;
            }
            else if (locations.Count > 0 && locations.All(x => x.Cleared))
            {
                // The item exists, but all locations are cleared
                Tracker.Say(x => x.Spoilers.LocationsCleared, item.NameWithArticle);
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
            var locationName = Tracker.WorldInfo.Location(location).Name;
            if (location.Cleared)
            {
                if (Tracker.HintsEnabled || Tracker.SpoilersEnabled)
                {
                    var itemName = _itemService.GetName(location.Item.Type);
                    Tracker.Say(x => x.Hints.LocationAlreadyClearedSpoiler, locationName, itemName);
                    return;
                }
                else
                {
                    Tracker.Say(x => x.Hints.LocationAlreadyCleared, locationName);
                    return;
                }
            }

            if (Tracker.MarkedLocations.TryGetValue(location.Id, out var markedItem))
            {
                Tracker.Say(x => x.Spoilers.MarkedLocation, locationName, markedItem.NameWithArticle);
                return;
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
            ItemData item, params object?[] additionalArgs)
        {
            var args = Args.Combine(item.NameWithArticle, additionalArgs);
            if (!Tracker.Say(responses => selectHint(responses.Hints), args))
                return false;

            if (_itemHintsGiven.ContainsKey(item.InternalItemType))
                _itemHintsGiven[item.InternalItemType]++;
            else
                _itemHintsGiven[item.InternalItemType] = 1;
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
            ItemData item, params object?[] additionalArgs)
        {
            var args = Args.Combine(item.NameWithArticle, additionalArgs);
            if (!Tracker.Say(hint, args))
                return false;

            if (_itemHintsGiven.ContainsKey(item.InternalItemType))
                _itemHintsGiven[item.InternalItemType]++;
            else
                _itemHintsGiven[item.InternalItemType] = 1;
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
            var name = Tracker.WorldInfo.Location(location).Name;
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
            var name = Tracker.WorldInfo.Location(location).Name;
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

                    if (ItemService.GetOrDefault(location.Item.Type)?.IsJunk(Tracker.World.Config) == true)
                    {
                        return GiveLocationHint(x => x.LocationHasJunkItem, location, characterName);
                    }

                    // Todo: Add check for if it's progression

                    if (ItemService.GetOrDefault(location.Item.Type)?.IsGood(Tracker.World.Config) == true)
                    {
                        return GiveLocationHint(x => x.LocationHasUsefulItem, location, characterName);
                    }

                    return GiveLocationHint(x => x.NoApplicableHints, location);

                // Try to give a hint from the config
                case 1:
                    var hint = ItemService.GetOrDefault(location)?.Hints;
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
            var locationName = Tracker.WorldInfo.Location(location).Name;
            if (location.Item == null || location.Item.Type == ItemType.Nothing)
            {
                Tracker.Say(x => x.Spoilers.EmptyLocation, locationName);
                return true;
            }

            var item = ItemService.GetOrDefault(location);
            if (item != null)
            {
                Tracker.Say(x => x.Spoilers.LocationHasItem, locationName, item.NameWithArticle);
                return true;
            }
            else
            {
                Tracker.Say(x => x.Spoilers.LocationHasUnknownItem, locationName, location.Item);
                return true;
            }
        }

        private bool GiveItemLocationSpoiler(ItemData item)
        {
            var reachableLocation = Tracker.World.Locations
                .Where(x => x.Item.Type == item.InternalItemType)
                .Where(x => !x.Cleared)
                .Where(x => x.IsAvailable(Tracker.GetProgression(x.Region)))
                .Random();
            if (reachableLocation != null)
            {
                var locationName = Tracker.WorldInfo.Location(reachableLocation).Name;
                var regionName = Tracker.WorldInfo.Region(reachableLocation.Region).Name;
                if (item.Multiple || item.HasStages)
                    Tracker.Say(x => x.Spoilers.ItemsAreAtLocation, item.NameWithArticle, locationName, regionName);
                else
                    Tracker.Say(x => x.Spoilers.ItemIsAtLocation, item.NameWithArticle, locationName, regionName);
                return true;
            }

            var worldLocation = Tracker.World.Locations
                .Where(x => x.Item.Type == item.InternalItemType)
                .Where(x => !x.Cleared)
                .Random();
            if (worldLocation != null)
            {
                var locationName = Tracker.WorldInfo.Location(worldLocation).Name;
                var regionName = Tracker.WorldInfo.Region(worldLocation.Region).Name;
                if (item.Multiple || item.HasStages)
                    Tracker.Say(x => x.Spoilers.ItemsAreAtOutOfLogicLocation, item.NameWithArticle, locationName, regionName);
                else
                    Tracker.Say(x => x.Spoilers.ItemIsAtOutOfLogicLocation, item.NameWithArticle, locationName, regionName);
                return true;
            }

            return false;
        }

        private bool GiveItemLocationHint(ItemData item)
        {
            var itemLocations = Tracker.World.Locations
                .Where(x => x.Item.Type == item.InternalItemType)
                .Where(x => !x.Cleared);

            if (!itemLocations.Any())
            {
                Logger.LogInformation("Can't find any uncleared locations with {Item}", item.InternalItemType.GetDescription());
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
                        var isInLogic = itemLocations.Any(x => x.IsAvailable(Tracker.GetProgression(x.Region)));
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
                        if (itemLocations.All(x => !x.IsAvailable(Tracker.GetProgression(x.Region))))
                        {
                            var randomLocation = itemLocations.Where(x => !x.IsAvailable(Tracker.GetProgression(x.Region))).Random();

                            if (randomLocation == null)
                            {
                                return GiveItemHint(x => x.NoApplicableHints, item);
                            }

                            var progression = Tracker.GetProgression(randomLocation.Region);
                            var missingItemSets = Logic.GetMissingRequiredItems(randomLocation, progression);
                            if (!missingItemSets.Any())
                            {
                                return GiveItemHint(x => x.ItemRequiresManyOtherItems, item);
                            }
                            else
                            {
                                var randomMissingItem = Logic.GetMissingRequiredItems(randomLocation, progression)
                                    .SelectMany(x => x)
                                    .Where(x => x != item.InternalItemType)
                                    .Select(x => ItemService.GetOrDefault(x))
                                    .Random();
                                if (randomMissingItem != null)
                                    return GiveItemHint(x => x.ItemRequiresOtherItem, item, randomMissingItem.NameWithArticle);
                            }
                        }

                        if (_playthrough == null)
                        {
                            return GiveItemHint(x => x.PlaythroughImpossible, item);
                        }

                        var sphere = _playthrough.Spheres.IndexOf(x => x.Items.Any(i => i.Type == item.InternalItemType));
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
                                          .Where(x => !x.Cleared)
                                          .Any(l => l.Item.Type == item.InternalItemType))
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
                        if (randomLocation?.Region is Z3Region and IHasReward dungeon && dungeon.Reward != RewardType.Agahnim)
                        {
                            if (randomLocation.Region.Locations.Any(x => x.Cleared))
                                return GiveItemHint(x => x.ItemInPreviouslyVisitedDungeon, item);
                            else
                                return GiveItemHint(x => x.ItemInUnvisitedDungeon, item);
                        }

                        if (randomLocation?.Region.Locations.Any(x => x.Cleared) == true)
                            return GiveItemHint(x => x.ItemInPreviouslyVisitedRegion, item);
                        else
                            return GiveItemHint(x => x.ItemInUnvisitedRegion, item);
                    }

                // Lv3 hints give a clue about the actual region it is in.
                case 3:
                    {
                        var randomLocationWithHint = GetRandomItemLocationWithFilter(item,
                            l => Tracker.WorldInfo.Region(l.Region).Hints?.Count > 0);
                        if (randomLocationWithHint != null)
                        {
                            var regionHint = Tracker.WorldInfo.Region(randomLocationWithHint.Region).Hints;
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
                                if (location.Room != null && Tracker.WorldInfo.Room(location.Room).Hints?.Count > 0)
                                    return true;
                                return Tracker.WorldInfo.Location(location).Hints?.Count > 0;
                            });

                        if (randomLocation != null)
                        {
                            if (randomLocation.Room != null)
                            {
                                var roomHint = Tracker.WorldInfo.Room(randomLocation.Room).Hints;
                                if (roomHint != null && roomHint.Count > 0)
                                {
                                    return GiveItemHint(roomHint, item);
                                }
                            }

                            var locationHint = Tracker.WorldInfo.Location(randomLocation).Hints;
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

                            var vanillaItem = ItemService.GetOrDefault(randomLocation.VanillaItem);
                            return GiveItemHint(x => x.ItemIsInVanillaJunkLocation, item, vanillaItem?.Name ?? randomLocation.VanillaItem.GetDescription());
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

        private int HintsGiven(ItemData item) => _itemHintsGiven.GetValueOrDefault(item.InternalItemType, 0);

        private int HintsGiven(Location location) => _locationHintsGiven.GetValueOrDefault(location.Id, 0);

        private Location? GetRandomItemLocationWithFilter(ItemData item, Func<Location, bool> predicate)
        {
            var randomLocation = Tracker.World.Locations
                .Where(x => x.Item.Type == item.InternalItemType)
                .Where(x => !x.Cleared)
                .Where(x => x.IsAvailable(Tracker.GetProgression(x.Region)))
                .Where(predicate)
                .Random();

            if (randomLocation == null)
            {
                // If the item is not at any accessible location, try to look in
                // out-of-logic places, too.
                randomLocation = Tracker.World.Locations
                    .Where(x => x.Item.Type == item.InternalItemType)
                    .Where(x => !x.Cleared)
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
