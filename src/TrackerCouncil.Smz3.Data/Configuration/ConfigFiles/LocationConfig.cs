using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using TrackerCouncil.Smz3.Data.Configuration.ConfigTypes;
using TrackerCouncil.Smz3.Shared.Enums;
using static TrackerCouncil.Smz3.Data.Configuration.ConfigTypes.SchrodingersString;

namespace TrackerCouncil.Smz3.Data.Configuration.ConfigFiles;

/// <summary>
/// Config file for additional location information
/// </summary>
[Description("Config file for location names and various tracker responses when clearing and marking locations")]
public class LocationConfig : List<LocationInfo>, IMergeable<LocationInfo>, IConfigFile<LocationConfig>
{
    /// <summary>
    /// Constructor
    /// </summary>
    public LocationConfig() : base()
    {
    }

    /// <summary>
    /// Returns default location information
    /// </summary>
    /// <returns></returns>
    public static LocationConfig Default()
    {
        var locationConfigs = new LocationConfig();
        locationConfigs.AddRange(Enum.GetValues<LocationId>().Select(val => new LocationInfo(val)));
        return locationConfigs;
    }

    public static object Example()
    {
        return new LocationConfig()
        {
            new()
            {
                LocationId = LocationId.GreenBrinstarGreenHillZone,
                LocationNumber = 25,
                Name =
                    new SchrodingersString("Green Hill Zone", "Jungle Slope",
                        new Possibility("Missile (green Brinstar pipe)", 0.1)),
                Hints =
                    new SchrodingersString("Vague hint that could be said when asking where an item is"),
                WhenTrackingJunk =
                    new SchrodingersString(
                        "Message when tracking the location and there's nothing good there"),
                WhenTrackingProgression =
                    new SchrodingersString(
                        "Message when tracking the location and there's an item that's potentially progression there"),
                WhenMarkingJunk =
                    new SchrodingersString("Message when marking a junk item at the location"),
                WhenMarkingProgression =
                    new SchrodingersString(
                        "Message when marking an item that's potentially progression at the location"),
                OutOfLogic =
                    new SchrodingersString("Message when tracking an item at the location out of logic")
            }
        };
    }
}
