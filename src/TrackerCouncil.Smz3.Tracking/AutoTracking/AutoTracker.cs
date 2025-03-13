using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SnesConnectorLibrary;
using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Data.Tracking;
using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.Tracking.AutoTracking.AutoTrackerModules;
using TrackerCouncil.Smz3.Tracking.Services;
using TrackerCouncil.Smz3.Tracking.VoiceCommands;

namespace TrackerCouncil.Smz3.Tracking.AutoTracking;

/// <summary>
/// Manages the automated checking of the emulator memory for purposes of auto tracking
/// and other things using the appropriate connector (USB2SNES or Lura) based on user
/// preferences.
/// </summary>
public class AutoTracker : AutoTrackerBase
{
    private readonly ILogger<AutoTracker> _logger;
    private readonly TrackerModuleFactory _trackerModuleFactory;
    private readonly IRandomizerConfigService _config;
    private bool _hasValidState;
    private int _numGTItems;
    private bool _foundGTKey;
    private ISnesConnectorService _snesConnectorService;
    private bool _isEnabled;
    private int _numDisconnects;
    private CancellationTokenSource? _validationCts;

    /// <summary>
    /// Constructor for Auto Tracker
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="trackerModuleFactory"></param>
    /// <param name="randomizerConfigService"></param>
    /// <param name="trackerBase"></param>
    /// <param name="snesConnectorService"></param>
    /// <param name="modules"></param>
    public AutoTracker(ILogger<AutoTracker> logger,
        TrackerModuleFactory trackerModuleFactory,
        IRandomizerConfigService randomizerConfigService,
        TrackerBase trackerBase,
        ISnesConnectorService snesConnectorService,
        IEnumerable<AutoTrackerModule> modules)
    {
        _logger = logger;
        _trackerModuleFactory = trackerModuleFactory;
        _config = randomizerConfigService;
        TrackerBase = trackerBase;
        _snesConnectorService = snesConnectorService;
        _snesConnectorService.ClearRequests();

        foreach (var module in modules)
        {
            module.AutoTracker = this;
            module.Initialize();
        }

        snesConnectorService.Connected += SnesConnectorServiceOnConnected;
        snesConnectorService.Disconnected += SnesConnectorServiceOnDisconnected;
    }

    private async void SnesConnectorServiceOnConnected(object? sender, EventArgs e)
    {
        try
        {
            _logger.LogInformation("Connector connected");
            _validationCts = new CancellationTokenSource();
            await Task.Delay(TimeSpan.FromSeconds(2), _validationCts.Token);

            if (!_validationCts.IsCancellationRequested)
            {
                _logger.LogInformation("Connection validated");
                TrackerBase.Say(x => x.AutoTracker.WhenConnected);
                OnAutoTrackerConnected();
                _numDisconnects = 0;
                _validationCts = null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Connector connection failure");
        }
    }

    private void SnesConnectorServiceOnDisconnected(object? sender, EventArgs e)
    {
        _logger.LogInformation("Connector Disconnected");

        if (_validationCts == null)
        {
            TrackerBase.Say(x => x.AutoTracker.WhenDisconnected);
            OnAutoTrackerDisconnected();
        }
        else
        {
            _numDisconnects++;
            _validationCts.Cancel();
        }

        HasStarted = false;
        CurrentGame = Game.Neither;
        _hasValidState = false;

        if (_numDisconnects > 10)
        {
            _logger.LogInformation("Disconnect limit of 10 reached");
            SetConnector(new SnesConnectorSettings(), SnesConnectorType.None);
            _numDisconnects = 0;
            TrackerBase.Say(x => x.AutoTracker.WhenDisconnectLimitReached);
        }
    }

    public override void SetConnector(SnesConnectorSettings snesConnectorSettings,
        SnesConnectorType? connectorTypeOverride = null)
    {
        if (connectorTypeOverride != null)
        {
            snesConnectorSettings.ConnectorType = connectorTypeOverride.Value;
        }

        var prevEnabled = IsEnabled;
        ConnectorType = snesConnectorSettings.ConnectorType;
        _isEnabled = snesConnectorSettings.ConnectorType != SnesConnectorType.None;

        if (prevEnabled != _isEnabled)
        {
            if (_isEnabled)
            {
                OnAutoTrackerEnabled();
            }
            else
            {
                OnAutoTrackerDisabled();
            }
        }

        OnAutoTrackerConnectorChanged();

        if (snesConnectorSettings.ConnectorType == SnesConnectorType.None)
        {
            _snesConnectorService.Disconnect();
        }
        else
        {
            _snesConnectorService.Connect(snesConnectorSettings);
        }
    }

    /// <summary>
    /// If a connector is currently enabled
    /// </summary>
    public override bool IsEnabled => _isEnabled;

    /// <summary>
    /// If a connector is currently connected to the emulator
    /// </summary>
    public override bool IsConnected => _snesConnectorService.IsConnected;

    /// <summary>
    /// If a connector is currently connected to the emulator and a valid game state is detected
    /// </summary>
    public override bool HasValidState => IsConnected && _hasValidState;

    /// <summary>
    /// If the user is activately in an SMZ3 rom
    /// </summary>
    public override bool IsInSMZ3 => CurrentGame is Game.Zelda or Game.SM or Game.Credits;

    public override void SetLatestViewAction(string key, Action action)
    {
        if (TrackerBase.Options.AutoSaveLookAtEvents)
        {
            if (LatestViewActionKey == key)
            {
                return;
            }

            LatestViewActionKey = key;

            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(TimeSpan.FromSeconds(0.5));
                action.Invoke();
                LatestViewActionKey = null;
            });
        }
        else
        {
            LatestViewAction = new AutoTrackerViewedAction(action);
        }
    }

    public override void UpdateGame(Game game)
    {
        PreviousGame = CurrentGame;
        CurrentGame = game;
    }

    public override void UpdateValidState(bool hasValidState)
    {
        _hasValidState = hasValidState;
    }

    public override void IncrementGTItems(Location location)
    {
        if (_foundGTKey || _config.Config.ZeldaKeysanity) return;

        var chatIntegrationModule = _trackerModuleFactory.GetModule<ChatIntegrationModule>();
        _numGTItems++;
        TrackerBase.Say(text: _numGTItems.ToString());
        if (location.Item.Type == ItemType.BigKeyGT)
        {
            TrackerBase.Say(selectResponses: x => x.AutoTracker.GTKeyResponses, tieredKey: _numGTItems, args: [_numGTItems]);
            chatIntegrationModule?.GTItemTracked(_numGTItems, true);
            _foundGTKey = true;
        }
        else
        {
            chatIntegrationModule?.GTItemTracked(_numGTItems, false);
        }
    }

    public override void Dispose()
    {
        _snesConnectorService.Connected -= SnesConnectorServiceOnConnected;
        _snesConnectorService.Disconnected -= SnesConnectorServiceOnDisconnected;
        _snesConnectorService.Disconnect();
        _snesConnectorService.ClearRequests();
    }
}
