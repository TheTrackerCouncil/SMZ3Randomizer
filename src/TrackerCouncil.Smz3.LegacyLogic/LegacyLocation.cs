using System;
using System.Collections.Generic;
using System.Linq;

namespace Randomizer.SMZ3 {

    enum LocationType {
        Regular,
        HeraStandingKey,
        Pedestal,
        Ether,
        Bombos,
        NotInDungeon,

        Visible,
        Chozo,
        Hidden
    }

    delegate bool Requirement(LegacyProgression items);
    delegate bool Verification(LegacyItem legacyItem, LegacyProgression items);

    class LegacyLocation {

        public int Id { get; set; }
        public string Name { get; set; }
        public LocationType Type { get; set; }
        public int Address { get; set; }
        public LegacyItem LegacyItem { get; set; }
        public LegacyRegion LegacyRegion { get; set; }

        public int Weight {
            get { return weight ?? LegacyRegion.Weight; }
        }

        readonly Requirement canAccess;
        Verification alwaysAllow;
        Verification allow;
        int? weight;

        public bool ItemIs(LegacyItemType type, LegacyWorld legacyWorld) => LegacyItem?.Is(type, legacyWorld) ?? false;
        public bool ItemIsNot(LegacyItemType type, LegacyWorld legacyWorld) => !ItemIs(type, legacyWorld);

        public LegacyLocation(LegacyRegion legacyRegion, int id, int address, LocationType type, string name)
            : this(legacyRegion, id, address, type, name, items => true) {
        }

        public LegacyLocation(LegacyRegion legacyRegion, int id, int address, LocationType type, string name, Requirement access) {
            LegacyRegion = legacyRegion;
            Id = id;
            Name = name;
            Type = type;
            Address = address;
            canAccess = access;
            alwaysAllow = (item, items) => false;
            allow = (item, items) => true;
        }

        public LegacyLocation Weighted(int? weight) {
            this.weight = weight;
            return this;
        }

        public LegacyLocation AlwaysAllow(Verification allow) {
            alwaysAllow = allow;
            return this;
        }

        public LegacyLocation Allow(Verification allow) {
            this.allow = allow;
            return this;
        }

        public bool Available(LegacyProgression items) {
            return LegacyRegion.CanEnter(items) && canAccess(items);
        }

        public bool CanFill(LegacyItem legacyItem, LegacyProgression items) {
            var oldItem = LegacyItem;
            LegacyItem = legacyItem;
            bool fillable = alwaysAllow(legacyItem, items) || (LegacyRegion.CanFill(legacyItem, items) && allow(legacyItem, items) && Available(items));
            LegacyItem = oldItem;
            return fillable;
        }

        // For logic unit tests
        internal bool CanAccess(LegacyProgression items) => canAccess(items);

    }

    static class LocationsExtensions {

        public static LegacyLocation Get(this IEnumerable<LegacyLocation> locations, string name) {
            var location = locations.FirstOrDefault(l => l.Name == name);
            if (location == null)
                throw new ArgumentException($"Could not find location name {name}", nameof(name));
            return location;
        }

        public static IEnumerable<LegacyLocation> Empty(this IEnumerable<LegacyLocation> locations) {
            return locations.Where(l => l.LegacyItem == null);
        }

        public static IEnumerable<LegacyLocation> Filled(this IEnumerable<LegacyLocation> locations) {
            return locations.Where(l => l.LegacyItem != null);
        }

        public static IEnumerable<LegacyLocation> AvailableWithinWorld(this IEnumerable<LegacyLocation> locations, IEnumerable<LegacyItem> items) {
            return locations.Select(x => x.LegacyRegion.LegacyWorld).Distinct().SelectMany(world =>
                locations.Where(l => l.LegacyRegion.LegacyWorld == world).Available(items.Where(i => i.LegacyWorld == world)));
        }

        public static IEnumerable<LegacyLocation> Available(this IEnumerable<LegacyLocation> locations, IEnumerable<LegacyItem> items) {
            var progression = new LegacyProgression(items);
            return locations.Where(l => l.Available(progression));
        }

        public static IEnumerable<LegacyLocation> CanFillWithinWorld(this IEnumerable<LegacyLocation> locations, LegacyItem legacyItem, IEnumerable<LegacyItem> items) {
            var itemWorldProgression = new LegacyProgression(items.Where(i => i.LegacyWorld == legacyItem.LegacyWorld).Append(legacyItem));
            var worldProgression = locations.Select(x => x.LegacyRegion.LegacyWorld).Distinct().ToDictionary(world => world.Id, world => new LegacyProgression(items.Where(i => i.LegacyWorld == world)));

            return locations.Where(l =>
                l.CanFill(legacyItem, worldProgression[l.LegacyRegion.LegacyWorld.Id]) &&
                legacyItem.LegacyWorld.Locations.Find(ll => ll.Id == l.Id).Available(itemWorldProgression));
        }

    }

}
