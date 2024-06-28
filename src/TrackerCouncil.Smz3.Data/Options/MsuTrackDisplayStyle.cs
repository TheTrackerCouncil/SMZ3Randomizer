using System.ComponentModel;

namespace TrackerCouncil.Smz3.Data.Options;

public enum MsuTrackDisplayStyle
{
    /// <summary>
    /// Original horizontal style: displays the current track without MSU pack
    /// info if artist information is available.
    /// </summary>
    [Description("Original: \"Cave Story+ - Pulse (Danny Baranowsky)\" or \"Track #5 from Sound of Silence\"")]
    Horizontal,

    /// <summary>
    /// Vertical style: separate lines for separate tracks, with MSU pack info.
    /// </summary>
    [Description("Multiple lines: MSU/album/song/artist")]
    Vertical,

    /// <summary>
    /// Horizontal style: displays the current track with MSU pack info.
    /// </summary>
    [Description("Single line: \"Cave Story+: Pulse - Danny Baranowsky (MSU: DBstyle by Vivelin)\"")]
    HorizonalWithMsu,

    /// <summary>
    /// Expanded sentence-style: displays track and MSU pack info in a single
    /// (long) line.
    /// </summary>
    [Description("Sentence: \"Pulse by Danny Baranowsky from album Cave Story+ from MSU pack DBstyle by Vivelin\"")]
    SentenceStyle,
}
