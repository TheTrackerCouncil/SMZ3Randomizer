using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Randomizer.SMZ3.Tracking
{
    public class TrackerConfig
    {
        public TrackerConfig(IReadOnlyCollection<ItemData> items)
        {
            Items = items;
        }

        public IReadOnlyCollection<ItemData> Items { get; init; }

        public static TrackerConfig GenerateDefault()
        {
            var itemData = Enum.GetValues<ItemType>()
                .Select(x => new ItemData(x.GetDescription(), x))
                .ToImmutableList();

            return new TrackerConfig(itemData);
        }

        public ItemData? FindItemByName(string name)
        {
            return Items.SingleOrDefault(x => x.Name == name)
                ?? Items.SingleOrDefault(x => x.OtherNames.Contains(name, StringComparer.OrdinalIgnoreCase))
                ?? Items.Where(x => x.Stages != null)
                        .SingleOrDefault(x => x.Stages!.SelectMany(stage => stage.Value).Any(x => x == name));
        }
    }
}
