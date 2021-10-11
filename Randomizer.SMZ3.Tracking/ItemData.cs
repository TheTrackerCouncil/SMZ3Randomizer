using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Randomizer.SMZ3.Tracking
{
    public class ItemData
    {
        public ItemData(string name, ItemType internalItemType)
        {
            Name = name;
            InternalItemType = internalItemType;
        }

        public string Name { get; init; }

        public ItemType InternalItemType { get; init; }

        public IReadOnlyCollection<string> OtherNames { get; init; }
            = new List<string>();

        public bool Multiple { get; init; }

        public IReadOnlyDictionary<int, IReadOnlyCollection<string>>? Stages { get; init; }

        [MemberNotNullWhen(true, nameof(Stages))]
        public bool HasStages => Stages != null && Stages.Count > 0;

        public int? GetStage(string name)
        {
            if (Stages?.Any(x => x.Value.Contains(name, StringComparer.OrdinalIgnoreCase)) == true)
                return Stages.Single(x => x.Value.Contains(name, StringComparer.OrdinalIgnoreCase)).Key;

            return null;
        }

        public string GetRandomName(Random random)
        {
            if (OtherNames.Count == 0)
                return Name;

            return OtherNames.Concat(new[] { Name }).Random(random);
        }
    }
}
