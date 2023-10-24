using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Speech.Recognition;
using System.Timers;
using Microsoft.Extensions.Logging;
using MSURandomizerLibrary;
using MSURandomizerLibrary.Configs;
using MSURandomizerLibrary.Models;
using MSURandomizerLibrary.Services;
using Randomizer.Abstractions;
using Randomizer.Data.Configuration.ConfigFiles;
using Randomizer.Data.Options;
using Randomizer.Data.Tracking;
using Randomizer.SMZ3.Tracking.Services;

namespace Randomizer.SMZ3.Tracking.VoiceCommands;

/// <summary>
/// Module for tracker stating what the current song is
/// </summary>
public class MsuModule : TrackerModule, IDisposable
{
    private readonly IMsuSelectorService _msuSelectorService;
    private Msu? _currentMsu;
    private readonly string? _msuPath;
    private readonly ICollection<string>? _inputMsuPaths;
    private readonly Timer? _timer;
    private readonly MsuType? _msuType;
    private readonly MsuConfig _msuConfig;
    private readonly string _msuKey = "MsuKey";
    private int _currentTrackNumber;
    private readonly HashSet<int> _validTrackNumbers;
    private Track? _currentTrack;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="tracker"></param>
    /// <param name="itemService">Service to get item information</param>
    /// <param name="worldService">Service to get world information</param>
    /// <param name="logger"></param>
    /// <param name="msuLookupService"></param>
    /// <param name="msuSelectorService"></param>
    /// <param name="msuTypeService"></param>
    /// <param name="msuConfig"></param>
    public MsuModule(ITracker tracker, IItemService itemService, IWorldService worldService, ILogger<MsuModule> logger, IMsuLookupService msuLookupService, IMsuSelectorService msuSelectorService, IMsuTypeService msuTypeService, MsuConfig msuConfig)
        : base(tracker, itemService, worldService, logger)
    {
        _msuSelectorService = msuSelectorService;
        _msuType = msuTypeService.GetSMZ3MsuType();
        _msuConfig = msuConfig;
        _validTrackNumbers = _msuType!.ValidTrackNumbers;

        if (!File.Exists(tracker.RomPath))
        {
            throw new InvalidOperationException("No tracker rom file found");
        }

        var romFileInfo = new FileInfo(tracker.RomPath);
        _msuPath = romFileInfo.FullName.Replace(romFileInfo.Extension, ".msu");

        if (!File.Exists(_msuPath))
        {
            return;
        }

        try
        {
            _currentMsu = msuLookupService.LoadMsu(_msuPath, _msuType, false, true, true);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Error loading MSU {Path}", _msuPath);
            return;
        }

        if (_currentMsu == null)
        {
            logger.LogWarning("MSU file found but unable to load MSU");
            return;
        }

        // Start reshuffling every minute if requested
        if (tracker.Rom!.MsuRandomizationStyle == MsuRandomizationStyle.Continuous)
        {
            _inputMsuPaths = tracker.Rom!.MsuPaths?.Split("|");
            _timer = new Timer(TimeSpan.FromSeconds(60));
            _timer.Elapsed += TimerOnElapsed;
            _timer.Start();
        }

        tracker.TrackNumberUpdated += TrackerOnTrackNumberUpdated;

    }

    private void TrackerOnTrackNumberUpdated(object? sender, TrackNumberEventArgs e)
    {
        if (!_validTrackNumbers.Contains(e.TrackNumber)) return;
        _currentTrackNumber = e.TrackNumber;
        if (_currentMsu == null) return;
        _currentTrack =_currentMsu.GetTrackFor(_currentTrackNumber);

        if (_currentTrack != null)
        {
            var output = GetOutputText();
            Tracker.UpdateTrack(_currentMsu, _currentTrack, output);

            if (!string.IsNullOrEmpty(Tracker.Options.MsuTrackOutputPath))
            {
                try
                {
                    _ = File.WriteAllTextAsync(Tracker.Options.MsuTrackOutputPath, output);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Unable to write current track details to {Path}", Tracker.Options.MsuTrackOutputPath);
                }
            }
        }

        Logger.LogInformation("Current Track: {Track}", _currentTrack?.GetDisplayText() ?? "Unknown");
    }

    private string GetOutputText()
    {
        if (_currentMsu == null || _currentTrack == null)
            return "";

        var options = Tracker.Options;
        if (options.MsuTrackDisplayStyle == MsuTrackDisplayStyle.Horizontal)
        {
            if (!string.IsNullOrEmpty(_currentTrack.DisplayAlbum) || !string.IsNullOrEmpty(_currentTrack.DisplayArtist))
            {
                var album = string.IsNullOrEmpty(_currentTrack.DisplayAlbum)
                    ? ""
                    : $"{_currentTrack.DisplayAlbum} - ";
                var artist = string.IsNullOrEmpty(_currentTrack.DisplayArtist)
                    ? ""
                    : $" ({_currentTrack.DisplayArtist})";
                return $"{album}{_currentTrack.SongName}{artist}";
            }
            else
            {
                var msu = string.IsNullOrEmpty(_currentTrack.MsuName)
                    ? _currentMsu.DisplayName
                    : _currentTrack.MsuName;
                return $"{_currentTrack.SongName} from {msu}";
            }
        }
        else
        {
            var lines = new List<string>();

            var creator = string.IsNullOrEmpty(_currentTrack.MsuCreator)
                ? _currentMsu.DisplayCreator
                : _currentTrack.MsuCreator;
            var msu = string.IsNullOrEmpty(_currentTrack.MsuName)
                ? _currentMsu.DisplayName
                : _currentTrack.MsuName;
            lines.Add(string.IsNullOrEmpty(creator)
                ? $"MSU: {msu}"
                : $"MSU: {msu} by {creator}");

            if (!string.IsNullOrEmpty(_currentTrack.DisplayAlbum))
                lines.Add($"Album: {_currentTrack.DisplayAlbum}");

            lines.Add($"Song: {_currentTrack.SongName}");

            if (!string.IsNullOrEmpty(_currentTrack.DisplayArtist))
                lines.Add($"Artist: {_currentTrack.DisplayArtist}");

            return string.Join("\r\n", lines);
        }
    }

    [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
    private GrammarBuilder GetLocationSongRules()
    {
        var msuLocations = new Choices();

        foreach (var track in _msuConfig.TrackLocations)
        {
            foreach (var name in track.Value)
            {
                msuLocations.Add(new SemanticResultValue(name, track.Key));
            }
        }

        var option1 = new GrammarBuilder()
            .Append("Hey tracker,")
            .OneOf("what's the current song for", "what's the song for", "what's the current theme for", "what's the theme for")
            .Optional("the")
            .Append(_msuKey, msuLocations);

        var option2 = new GrammarBuilder()
            .Append("Hey tracker,")
            .OneOf("what's the current", "what's the")
            .Append(_msuKey, msuLocations)
            .OneOf("song", "theme");

        return GrammarBuilder.Combine(option1, option2);

    }

    [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
    private GrammarBuilder GetLocationMsuRules()
    {
        var msuLocations = new Choices();

        foreach (var track in _msuConfig.TrackLocations)
        {
            foreach (var name in track.Value)
            {
                msuLocations.Add(new SemanticResultValue(name, track.Key));
            }
        }

        var option1 = new GrammarBuilder()
            .Append("Hey tracker,")
            .Append("what MSU pack is")
            .OneOf("the current song for", "the song for", "the current theme for", "the theme for")
            .Optional("the")
            .Append(_msuKey, msuLocations)
            .Append("from");

        var option2 = new GrammarBuilder()
            .Append("Hey tracker,")
            .Append("what MSU pack is")
            .OneOf("the current", "the")
            .Append(_msuKey, msuLocations)
            .OneOf("song", "theme")
            .Append("from");

        return GrammarBuilder.Combine(option1, option2);

    }

    private string GetTrackText(Track track)
    {
        var parts = new List<string>() { track.SongName };
        if (!string.IsNullOrEmpty(track.DisplayAlbum))
        {
            parts.Add($"from the album {track.DisplayAlbum}");
        }
        else if (!string.IsNullOrEmpty(track.DisplayArtist))
        {
            parts.Add($"by {track.DisplayArtist}");
        }
        if (!string.IsNullOrEmpty(track.MsuName) && Tracker.Rom!.MsuRandomizationStyle != null)
        {
            parts.Add($"from MSU Pack {track.MsuName}");
            if (!string.IsNullOrEmpty(track.MsuCreator)) parts.Add($"by {track.MsuCreator}");
        }

        return string.Join("; ", parts);
    }

    [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
    private GrammarBuilder GetCurrentSongRules()
    {
        var msuLocations = new Choices();

        foreach (var track in _msuConfig.TrackLocations)
        {
            foreach (var name in track.Value)
            {
                msuLocations.Add(new SemanticResultValue(name, track.Key));
            }
        }

        return new GrammarBuilder()
            .Append("Hey tracker,")
            .OneOf("what's the current song", "what's currently playing", "what's the current track");

    }

    private GrammarBuilder GetCurrentMsuRules()
    {
        return new GrammarBuilder()
            .Append("Hey tracker,")
            .Append("what MSU pack is")
            .OneOf("the current song from", "the current track from", "the current theme from");
    }

    private void TimerOnElapsed(object? sender, ElapsedEventArgs e)
    {
        try
        {
            var response = _msuSelectorService.CreateShuffledMsu(new MsuSelectorRequest()
            {
                MsuPaths = _inputMsuPaths, OutputMsuType = _msuType, OutputPath = _msuPath, PrevMsu = _currentMsu
            });
            _currentMsu = response.Msu;
        }
        catch (Exception exception)
        {
            Logger.LogError(exception, "Error creating MSU");
        }

        Tracker.GameService?.TryCancelMsuResume();
    }

    public void Dispose()
    {
        if (_timer != null)
        {
            _timer.Stop();
            _timer.Dispose();
        }
    }

    [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
    public override void AddCommands()
    {
        AddCommand("location song", GetLocationSongRules(), (result) =>
        {
            if (_currentMsu == null)
            {
                Tracker.Say(_msuConfig.UnknownSong);
                return;
            }

            var trackNumber = (int)result.Semantics[_msuKey].Value;
            var track = _currentMsu.GetTrackFor(trackNumber);
            if (track != null)
            {
                Tracker.Say(_msuConfig.CurrentSong, GetTrackText(track));
            }
            else
            {
                Tracker.Say(_msuConfig.UnknownSong);
            }
        });

        AddCommand("location msu", GetLocationMsuRules(), (result) =>
        {
            if (_currentMsu == null)
            {
                Tracker.Say(_msuConfig.UnknownSong);
                return;
            }

            var trackNumber = (int)result.Semantics[_msuKey].Value;
            var track = _currentMsu.GetTrackFor(trackNumber);
            if (track?.GetMsuName() != null)
            {
                Tracker.Say(_msuConfig.CurrentMsu, track.GetMsuName());
            }
            else
            {
                Tracker.Say(_msuConfig.UnknownSong);
            }
        });

        AddCommand("current song", GetCurrentSongRules(), (_) =>
        {
            if (_currentTrack == null)
            {
                Tracker.Say(_msuConfig.UnknownSong);
                return;
            }
            Tracker.Say(_msuConfig.CurrentSong, GetTrackText(_currentTrack));
        });

        AddCommand("current msu", GetCurrentMsuRules(), (_) =>
        {
            if (_currentTrack == null)
            {
                Tracker.Say(_msuConfig.UnknownSong);
                return;
            }
            Tracker.Say(_msuConfig.CurrentMsu, _currentTrack.GetMsuName());
        });
    }
}
