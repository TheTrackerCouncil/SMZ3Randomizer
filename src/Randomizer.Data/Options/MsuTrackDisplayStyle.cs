using System.ComponentModel;

namespace Randomizer.Data.Options;

public enum MsuTrackDisplayStyle
{
    /// <summary>
    /// Original horizontal style: displays the current track without MSU pack
    /// info if artist information is available.
    /// </summary>
    [Description("Original: \"album - track (artist)\" or \"track - MSU pack name\"")]
    Horizontal,

    /// <summary>
    /// Vertical style: separate lines for separate tracks, with MSU pack info.
    /// </summary>
    [Description("Multiple lines: MSU/album/song/artist")]
    Vertical,

    /// <summary>
    /// Horizontal style: displays the current track with MSU pack info.
    /// </summary>
    [Description("Single line: \"album: track - artist (MSU: name by creator)\"")]
    HorizonalWithMsu,

    /// <summary>
    /// Expanded sentence-style: displays track and MSU pack info in a single
    /// (long) line.
    /// </summary>
    [Description("Sentence: \"track by artist from album album from MSU pack name by creator\"")]
    SentenceStyle,
}
