using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Speech.Recognition;
using Microsoft.Extensions.Logging;
using MSURandomizerLibrary;
using MSURandomizerLibrary.Configs;
using MSURandomizerLibrary.Models;
using MSURandomizerLibrary.Services;
using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Data.Configuration.ConfigFiles;
using TrackerCouncil.Smz3.Tracking.Services;

namespace TrackerCouncil.Smz3.Tracking.VoiceCommands;

/// <summary>
/// Module for tracker stating what the current song is
/// </summary>
public class MsuModule : TrackerModule, IDisposable
{
    private Msu? _currentMsu;
    private readonly MsuConfig _msuConfig;
    private readonly IMsuMonitorService _msuMonitorService;
    private readonly IGameService _gameService;
    private readonly string _msuKey = "MsuKey";
    private int _currentTrackNumber;
    private readonly HashSet<int> _validTrackNumbers;
    private Track? _currentTrack;
    private readonly bool _isSetup;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="tracker"></param>
    /// <param name="itemService">Service to get item information</param>
    /// <param name="worldService">Service to get world information</param>
    /// <param name="logger"></param>
    /// <param name="msuLookupService"></param>
    /// <param name="msuMonitorService"></param>
    /// <param name="msuTypeService"></param>
    /// <param name="msuUserOptionsService"></param>
    /// <param name="msuConfig"></param>
    /// <param name="gameService"></param>
    public MsuModule(
        TrackerBase tracker,
        IItemService itemService,
        IWorldService worldService,
        ILogger<MsuModule> logger,
        IMsuLookupService msuLookupService,
        IMsuMonitorService msuMonitorService,
        IMsuTypeService msuTypeService,
        IMsuUserOptionsService msuUserOptionsService,
        MsuConfig msuConfig,
        IGameService gameService)
        : base(tracker, itemService, worldService, logger)
    {
        _gameService = gameService;
        _msuMonitorService = msuMonitorService;
        var msuType = msuTypeService.GetSMZ3MsuType();
        _msuConfig = msuConfig;
        _validTrackNumbers = msuType!.ValidTrackNumbers;

        if (!File.Exists(tracker.RomPath))
        {
            throw new InvalidOperationException("No tracker rom file found");
        }

        if (string.IsNullOrEmpty(tracker.Rom?.MsuPaths))
        {
            return;
        }

        var romFileInfo = new FileInfo(tracker.RomPath);
        var msuPath = romFileInfo.FullName.Replace(romFileInfo.Extension, ".msu");

        if (!File.Exists(msuPath))
        {
            return;
        }

        try
        {
            _currentMsu = msuLookupService.LoadMsu(msuPath, msuType, false, true, true);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Error loading MSU {Path}", msuPath);
            return;
        }

        if (_currentMsu == null)
        {
            logger.LogWarning("MSU file found but unable to load MSU");
            return;
        }

        msuUserOptionsService.MsuUserOptions.MsuCurrentSongOutputFilePath = TrackerBase.Options.MsuTrackOutputPath;
        msuUserOptionsService.MsuUserOptions.TrackDisplayFormat = TrackerBase.Options.TrackDisplayFormat;
        msuUserOptionsService.MsuUserOptions.MsuShuffleStyle =
            tracker.Rom!.MsuShuffleStyle ?? MsuShuffleStyle.StandardShuffle;

        if (tracker.Rom!.MsuRandomizationStyle == MsuRandomizationStyle.Continuous)
        {
            var inputMsus = tracker.Rom!.MsuPaths?.Split("|");
            msuMonitorService.StartShuffle(new MsuSelectorRequest()
            {
                MsuPaths = inputMsus,
                OutputMsuType = msuType,
                OutputPath = msuPath,
                PrevMsu = _currentMsu,
                ShuffleStyle = msuUserOptionsService.MsuUserOptions.MsuShuffleStyle
            });
        }
        else
        {
            msuMonitorService.StartMonitor(_currentMsu, msuType);
        }

        msuMonitorService.MsuTrackChanged += MsuMonitorServiceOnMsuTrackChanged;
        msuMonitorService.MsuShuffled += MsuMonitorServiceOnMsuShuffled;

        _isSetup = true;

    }

    private void MsuMonitorServiceOnMsuShuffled(object? sender, EventArgs e)
    {
        _gameService.TryCancelMsuResume();
    }

    private void MsuMonitorServiceOnMsuTrackChanged(object sender, MsuTrackChangedEventArgs e)
    {
        if (!_validTrackNumbers.Contains(e.Track.Number)) return;
        _currentTrack = e.Track;
        _currentTrackNumber = e.Track.Number;

        // Respond if we have lines to the song number, song name, or msu name
        if (_msuConfig.SongResponses?.TryGetValue(_currentTrack.MsuName ?? "", out var response) == true)
        {
            TrackerBase.Say(response);
        }
        else if (_msuConfig.SongResponses?.TryGetValue(_currentTrack.SongName, out response) == true)
        {
            TrackerBase.Say(response);
        }
        else if (_msuConfig.SongResponses?.TryGetValue(_currentTrackNumber.ToString(), out response) == true)
        {
            TrackerBase.Say(response);
        }
    }

    [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
    private GrammarBuilder GetLocationSongRules()
    {
        if (_msuConfig.TrackLocations == null)
        {
            return new GrammarBuilder();
        }

        var msuLocations = new Choices();

        foreach (var track in _msuConfig.TrackLocations)
        {
            if (track.Value == null)
            {
                continue;
            }
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
        if (_msuConfig.TrackLocations == null)
        {
            return new GrammarBuilder();
        }

        var msuLocations = new Choices();

        foreach (var track in _msuConfig.TrackLocations)
        {
            if (track.Value == null)
            {
                continue;
            }

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

    [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
    private GrammarBuilder GetCurrentSongRules()
    {
        if (_msuConfig.TrackLocations == null)
        {
            return new GrammarBuilder();
        }

        var msuLocations = new Choices();

        foreach (var track in _msuConfig.TrackLocations)
        {
            if (track.Value == null)
            {
                continue;
            }

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

    public void Dispose()
    {
        _msuMonitorService.MsuTrackChanged -= MsuMonitorServiceOnMsuTrackChanged;
        _msuMonitorService.Stop();
    }

    [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
    public override void AddCommands()
    {
        if (!_isSetup)
        {
            return;
        }

        AddCommand("location song", GetLocationSongRules(), (result) =>
        {
            if (_currentMsu == null)
            {
                TrackerBase.Say(_msuConfig.UnknownSong);
                return;
            }

            var trackNumber = (int)result.Semantics[_msuKey].Value;
            var track = _currentMsu.GetTrackFor(trackNumber);
            if (track != null)
            {
                TrackerBase.Say(_msuConfig.CurrentSong, track.GetDisplayText(TrackDisplayFormat.SpeechStyle));
            }
            else
            {
                TrackerBase.Say(_msuConfig.UnknownSong);
            }
        });

        AddCommand("location msu", GetLocationMsuRules(), (result) =>
        {
            if (_currentMsu == null)
            {
                TrackerBase.Say(_msuConfig.UnknownSong);
                return;
            }

            var trackNumber = (int)result.Semantics[_msuKey].Value;
            var track = _currentMsu.GetTrackFor(trackNumber);
            if (track?.GetMsuName() != null)
            {
                TrackerBase.Say(_msuConfig.CurrentMsu, track.GetMsuName());
            }
            else
            {
                TrackerBase.Say(_msuConfig.UnknownSong);
            }
        });

        AddCommand("current song", GetCurrentSongRules(), (_) =>
        {
            if (_currentTrack == null)
            {
                TrackerBase.Say(_msuConfig.UnknownSong);
                return;
            }
            TrackerBase.Say(_msuConfig.CurrentSong, _currentTrack.GetDisplayText(TrackDisplayFormat.SpeechStyle));
        });

        AddCommand("current msu", GetCurrentMsuRules(), (_) =>
        {
            if (_currentTrack == null)
            {
                TrackerBase.Say(_msuConfig.UnknownSong);
                return;
            }
            TrackerBase.Say(_msuConfig.CurrentMsu, _currentTrack.GetMsuName());
        });
    }
}
