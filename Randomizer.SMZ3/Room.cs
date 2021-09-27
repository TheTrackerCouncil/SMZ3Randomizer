using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Randomizer.SMZ3
{
    public class Room
    {
        public Room(Region region, string name)
            : this(region, name, Array.Empty<string>())
        {
        }

        public Room(Region region, string name, params string[] alsoKnownAs)
        {
            Region = region;
            Name = name;
            AlsoKnownAs = new ReadOnlyCollection<string>(alsoKnownAs);
        }

        public Region Region { get; }

        public string Name { get; }

        public IReadOnlyCollection<string> AlsoKnownAs { get; }
    }
}
