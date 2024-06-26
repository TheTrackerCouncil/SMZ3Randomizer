using System.Collections.Generic;
using System.ComponentModel;
using TrackerCouncil.Data.Configuration;
using TrackerCouncil.Smz3.Data.Configuration.ConfigTypes;

namespace TrackerCouncil.Smz3.Data.Configuration.ConfigFiles;

/// <summary>
/// Config file for msu track names, responses for when asking about a song, and various tracker responses to songs playing
/// </summary>
[Description("Config file for msu track names, responses for when asking about a song, and various tracker responses to songs playing")]
public class MsuConfig : IMergeable<MsuConfig>, IConfigFile<MsuConfig>
{
    /// <summary>
    /// Names for various tracks when asking "what's the current song for"
    /// </summary>
    public Dictionary<int, SchrodingersString?>? TrackLocations { get; init; }

    /// <summary>
    /// Gets the phrases for what song is playing
    /// <c>{0}</c> is a placeholder for the song details
    /// </summary>
    public SchrodingersString? CurrentSong { get; init; }

    /// <summary>
    /// Gets the phrases for what msu the current song is from
    /// <c>{0}</c> is a placeholder for the msu details
    /// </summary>
    public SchrodingersString? CurrentMsu { get; init; }

    /// <summary>
    /// Gets the phrases for not being able to determine the playing song
    /// </summary>
    public SchrodingersString? UnknownSong { get; init; }

    /// <summary>
    /// Responses to songs, either by song number, msu name, or song name
    /// </summary>
    public Dictionary<string, SchrodingersString?>? SongResponses { get; init; }

    /// <summary>
    /// Returns default response information
    /// </summary>
    /// <returns></returns>
    public static MsuConfig Default()
    {
        return new MsuConfig();
    }

    public static object Example()
    {
        return new Dictionary<string, object>()
        {
            {
                "TrackLocations",
                new Dictionary<int, SchrodingersString>()
                {
                    { 2, new SchrodingersString("Song name for track #2 (Hyrule Field)", new SchrodingersString.Possibility("Another song name for track #2", 0.1))}
                }
            },
            {
                "CurrentSong", new SchrodingersString("Line for Tracker telling you that {0} is the current song", new SchrodingersString.Possibility("Another line for Tracker stating that {0} is the current song"))
            },
            {
                "SongResponses",
                new Dictionary<string, SchrodingersString>()
                {
                    { "MSU or Track Name", new SchrodingersString("Line for tracker to say when hearing a song with either the MSU or track name", new SchrodingersString.Possibility("Another possible tracker response", 0.1)) }
                }
            }
        };
    }
}
