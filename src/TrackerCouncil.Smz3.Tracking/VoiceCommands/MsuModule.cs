using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Speech.Recognition;
using Microsoft.Extensions.Logging;
using MSURandomizerLibrary;
using MSURandomizerLibrary.Configs;
using MSURandomizerLibrary.Messenger;
using MSURandomizerLibrary.Models;
using MSURandomizerLibrary.Services;
using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Data.Configuration;
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
    private readonly IMsuLookupService _msuLookupService;
    private readonly IMsuMessageReceiver _msuMessageReceiver;
    private readonly IMsuUserOptionsService _msuUserOptionsService;
    private readonly string _msuKey = "MsuKey";
    private int _currentTrackNumber;
    private readonly HashSet<int> _validTrackNumbers;
    private Track? _currentTrack;
    private readonly bool _isSetup;
    private readonly bool _isLocal;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="tracker"></param>
    /// <param name="playerProgressionService">Service to get item information</param>
    /// <param name="worldQueryService">Service to get world information</param>
    /// <param name="logger"></param>
    /// <param name="msuLookupService"></param>
    /// <param name="msuMonitorService"></param>
    /// <param name="msuTypeService"></param>
    /// <param name="msuUserOptionsService"></param>
    /// <param name="config"></param>
    /// <param name="gameService"></param>
    /// <param name="msuMessageReceiver"></param>
    public MsuModule(
        TrackerBase tracker,
        IPlayerProgressionService playerProgressionService,
        IWorldQueryService worldQueryService,
        ILogger<MsuModule> logger,
        IMsuLookupService msuLookupService,
        IMsuMonitorService msuMonitorService,
        IMsuTypeService msuTypeService,
        IMsuUserOptionsService msuUserOptionsService,
        Configs config,
        IGameService gameService,
        IMsuMessageReceiver msuMessageReceiver)
        : base(tracker, playerProgressionService, worldQueryService, logger)
    {
        _gameService = gameService;
        _msuMessageReceiver = msuMessageReceiver;
        _msuMonitorService = msuMonitorService;
        _msuLookupService = msuLookupService;
        _msuUserOptionsService = msuUserOptionsService;
        var msuType = msuTypeService.GetSMZ3MsuType();
        _msuConfig = config.MsuConfig;
        _validTrackNumbers = msuType!.ValidTrackNumbers;

        if (!File.Exists(tracker.RomPath))
        {
            throw new InvalidOperationException("No tracker rom file found");
        }

        if (!string.IsNullOrEmpty(tracker.Rom?.MsuPaths))
        {
            InitializeLocalMsuSupport(msuType);
            _isLocal = true;
        }
        else if (tracker.Options.MsuMessageReceiverEnabled)
        {
            InitializeRemoteMsuSupport();
        }

        _isSetup = true;
    }

    private void InitializeLocalMsuSupport(MsuType msuType)
    {
        if (string.IsNullOrEmpty(TrackerBase.RomPath))
        {
            return;
        }

        var romFileInfo = new FileInfo(TrackerBase.RomPath);
        var msuPath = romFileInfo.FullName.Replace(romFileInfo.Extension, ".msu");

        if (!File.Exists(msuPath))
        {
            return;
        }

        try
        {
            _currentMsu = _msuLookupService.LoadMsu(msuPath, msuType, false, true, true);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Error loading MSU {Path}", msuPath);
            return;
        }

        if (_currentMsu == null)
        {
            Logger.LogWarning("MSU file found but unable to load MSU");
            return;
        }

        _msuUserOptionsService.MsuUserOptions.MsuCurrentSongOutputFilePath = TrackerBase.Options.MsuTrackOutputPath;
        _msuUserOptionsService.MsuUserOptions.TrackDisplayFormat = TrackerBase.Options.TrackDisplayFormat;
        _msuUserOptionsService.MsuUserOptions.MsuShuffleStyle =
            TrackerBase.Rom!.MsuShuffleStyle ?? MsuShuffleStyle.StandardShuffle;

        if (TrackerBase.Rom!.MsuRandomizationStyle == MsuRandomizationStyle.Continuous)
        {
            var inputMsus = TrackerBase.Rom!.MsuPaths?.Split("|");
            _msuMonitorService.StartShuffle(new MsuSelectorRequest()
            {
                MsuPaths = inputMsus,
                OutputMsuType = msuType,
                OutputPath = msuPath,
                PrevMsu = _currentMsu,
                ShuffleStyle = _msuUserOptionsService.MsuUserOptions.MsuShuffleStyle
            });
        }
        else
        {
            _msuMonitorService.StartMonitor(_currentMsu, msuType);
        }

        _msuMonitorService.MsuTrackChanged += MsuMonitorServiceOnMsuTrackChanged;
        _msuMonitorService.PreMsuShuffle += MsuMonitorServiceOnPreMsuShuffle;
    }

    private void InitializeRemoteMsuSupport()
    {
        _msuMessageReceiver.Initialize();
        _msuMessageReceiver.TrackChanged += MsuMonitorServiceOnMsuTrackChanged;
        _msuMessageReceiver.MsuGenerated += MsuMonitorServiceOnPreMsuShuffle;
    }

    private void MsuMonitorServiceOnPreMsuShuffle(object? sender, EventArgs e)
    {
        Logger.LogInformation("Msu Reshuffling - Try Cancel MSU Resume");
        _gameService.TryCancelMsuResume();
    }

    private void MsuMonitorServiceOnMsuTrackChanged(object? sender, MsuTrackChangedEventArgs e)
    {
        if (!_validTrackNumbers.Contains(e.Track.Number)) return;

        _currentTrack = e.Track;
        _currentTrackNumber = e.Track.Number;

        Logger.LogDebug("Msu track changed to: {SongName}", e.Track.GetDisplayText(TrackDisplayFormat.Horizontal));

        // Respond if we have lines to the song number, song name, or msu name
        if (_msuConfig.SongResponses?.TryGetValue(_currentTrack.MsuName ?? "", out var response) == true)
        {
            TrackerBase.Say(response: response);
        }
        else if (_msuConfig.SongResponses?.TryGetValue(_currentTrack.SongName, out response) == true)
        {
            TrackerBase.Say(response: response);
        }
        else if (_msuConfig.SongResponses?.TryGetValue(_currentTrackNumber.ToString(), out response) == true)
        {
            TrackerBase.Say(response: response);
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
        _msuMessageReceiver.TrackChanged -= MsuMonitorServiceOnMsuTrackChanged;
        _msuMessageReceiver.MsuGenerated -= MsuMonitorServiceOnPreMsuShuffle;
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

        if (_isLocal)
        {
            AddCommand("location song", GetLocationSongRules(), (result) =>
            {
                if (_currentMsu == null)
                {
                    TrackerBase.Say(response: _msuConfig.UnknownSong);
                    return;
                }

                var trackNumber = (int)result.Semantics[_msuKey].Value;
                var track = _currentMsu.GetTrackFor(trackNumber);
                if (track != null)
                {
                    TrackerBase.Say(response: _msuConfig.CurrentSong, args: [track.GetDisplayText(TrackDisplayFormat.SpeechStyle)]);
                }
                else
                {
                    TrackerBase.Say(response: _msuConfig.UnknownSong);
                }
            });

            AddCommand("location msu", GetLocationMsuRules(), (result) =>
            {
                if (_currentMsu == null)
                {
                    TrackerBase.Say(response: _msuConfig.UnknownSong);
                    return;
                }

                var trackNumber = (int)result.Semantics[_msuKey].Value;
                var track = _currentMsu.GetTrackFor(trackNumber);
                if (track?.GetMsuName() != null)
                {
                    TrackerBase.Say(response: _msuConfig.CurrentMsu, args: [track.GetMsuName()]);
                }
                else
                {
                    TrackerBase.Say(response: _msuConfig.UnknownSong);
                }
            });
        }

        AddCommand("current song", GetCurrentSongRules(), (_) =>
        {
            if (_currentTrack == null)
            {
                TrackerBase.Say(response: _msuConfig.UnknownSong);
                return;
            }
            TrackerBase.Say(response: _msuConfig.CurrentSong, args: [_currentTrack.GetDisplayText(TrackDisplayFormat.SpeechStyle)]);
        });

        AddCommand("current msu", GetCurrentMsuRules(), (_) =>
        {
            if (_currentTrack == null)
            {
                TrackerBase.Say(response: _msuConfig.UnknownSong);
                return;
            }
            TrackerBase.Say(response: _msuConfig.CurrentMsu, args: [_currentTrack.GetMsuName()]);
        });
    }
}
