using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Randomizer.SMZ3.Tracking.Vocabulary;

namespace Randomizer.SMZ3.Tracking
{
    public static class LocationExtensions
    {
        public static SchrodingersString GetName(this Location location)
        {
            var names = new SchrodingersString();
            names.Add(location.Name);
            foreach (var alt in location.AlternateNames)
                names.Add(alt);
            return names;
        }
    }
}
