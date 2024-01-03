using System.Text;

using MSURandomizerLibrary.Configs;

namespace Randomizer.SMZ3.Tracking;

/// <summary>
/// Provides a fluent interface for building MSU display text.
/// </summary>
public class MsuDisplayTextBuilder
{
    private readonly StringBuilder _builder = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="MsuDisplayTextBuilder"/>
    /// class.
    /// </summary>
    /// <param name="track">The track to be formatted.</param>
    /// <param name="msu">The MSU to be formatted.</param>
    public MsuDisplayTextBuilder(Track track, Msu msu)
    {
        Track = track;
        Msu = msu;
    }

    /// <summary>
    /// Gets or sets the Track to be formatted.
    /// </summary>
    public Track Track { get; set; }

    /// <summary>
    /// Gets or sets the MSU to be formatted.
    /// </summary>
    public Msu Msu { get; set; }

    /// <summary>
    /// Adds the album name in the specified format if it is available.
    /// </summary>
    /// <param name="format">
    /// The format string, where <c>{0}</c> will be replaced with the album
    /// name.
    /// </param>
    /// <param name="fallback">
    /// The text to add if the album name is not available.
    /// </param>
    /// <returns>
    /// This instance with the formatted album name added if it is available.
    /// </returns>
    public MsuDisplayTextBuilder AddAlbum(string format, string? fallback = "")
    {
        if (!string.IsNullOrWhiteSpace(Track.DisplayAlbum))
            _builder.AppendFormat(format, Track.DisplayAlbum);
        else
            _builder.Append(fallback);

        return this;
    }

    /// <summary>
    /// Adds the artist name in the specified format if it is available.
    /// </summary>
    /// <param name="format">
    /// The format string, where <c>{0}</c> will be replaced with the artist
    /// name.
    /// </param>
    /// <param name="fallback">
    /// The text to add if the artist name is not available.
    /// </param>
    /// <returns>
    /// This instance with the formatted artist name added if it is available.
    /// </returns>
    public MsuDisplayTextBuilder AddArtist(string format, string? fallback = "")
    {
        if (!string.IsNullOrWhiteSpace(Track.DisplayArtist))
            _builder.AppendFormat(format, Track.DisplayArtist);
        else
            _builder.Append(fallback);

        return this;
    }

    /// <summary>
    /// Adds the song name in the specified format if it is available.
    /// </summary>
    /// <param name="format">
    /// The format string, where <c>{0}</c> will be replaced with the song name.
    /// </param>
    /// <param name="fallback">
    /// The text to add if the song name is not available.
    /// </param>
    /// <returns>
    /// This instance with the formatted song name added if it is available.
    /// </returns>
    public MsuDisplayTextBuilder AddTrackTitle(string format, string? fallback = "")
    {
        if (!string.IsNullOrWhiteSpace(Track.SongName))
            _builder.AppendFormat(format, Track.SongName);
        else
            _builder.Append(fallback);

        return this;
    }

    /// <summary>
    /// Adds the MSU pack name and author together in the specified format if it
    /// is available.
    /// </summary>
    /// <param name="format">
    /// The format string, where <c>{0}</c> will be replaced with the MSU pack
    /// name and author.
    /// </param>
    /// <param name="fallback">
    /// The text to add if the MSU pack name or creator are not available.
    /// </param>
    /// <returns>
    /// This instance with the formatted MSU pack name and author added if it is
    /// available.
    /// </returns>
    public MsuDisplayTextBuilder AddMsuNameAndCreator(string format, string? fallback = "")
    {
        var fullName = Track.GetMsuName();
        if (!string.IsNullOrWhiteSpace(fullName))
            _builder.AppendFormat(format, fullName);
        else
            _builder.Append(fallback);

        return this;
    }

    /// <summary>
    /// Adds the MSU pack name in the specified format if it is available.
    /// </summary>
    /// <param name="format">
    /// The format string, where <c>{0}</c> will be replaced with the MSU pack
    /// name.
    /// </param>
    /// <param name="fallback">
    /// The text to add if the MSU pack name is not available.
    /// </param>
    /// <returns>
    /// This instance with the formatted MSU pack name added if it is available.
    /// </returns>
    public MsuDisplayTextBuilder AddMsuName(string format, string? fallback = "")
    {
        if (!string.IsNullOrWhiteSpace(Track.MsuName))
            _builder.AppendFormat(format, Track.MsuName);
        else
            _builder.Append(fallback);

        return this;
    }

    /// <summary>
    /// Adds the MSU author in the specified format if it is available.
    /// </summary>
    /// <param name="format">
    /// The format string, where <c>{0}</c> will be replaced with the MSU
    /// author.
    /// </param>
    /// <param name="fallback">
    /// The text to add if the MSU author is not available.
    /// </param>
    /// <returns>
    /// This instance with the formatted MSU author added if it is available.
    /// </returns>
    public MsuDisplayTextBuilder AddMsuCreator(string format, string? fallback = "")
    {
        if (!string.IsNullOrWhiteSpace(Track.MsuCreator))
            _builder.AppendFormat(format, Track.MsuCreator);
        else
            _builder.Append(fallback);

        return this;
    }

    /// <summary>
    /// Returns the formatted MSU display text.
    /// </summary>
    /// <returns>A string containing the formatted text.</returns>
    public override string ToString() => _builder.ToString().Trim();
}
