using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Randomizer.SMZ3.Tracking.Vocabulary;

namespace Randomizer.SMZ3.Tracking
{
    public static class SchrodingersStringExtensions
    {
        public static SchrodingersString GetName(this IHasLocations area)
        {
            var names = new SchrodingersString();
            names.Add(area.Name);
            foreach (var name in area.AlsoKnownAs)
                names.Add(name);
            return names;
        }
    }
}
