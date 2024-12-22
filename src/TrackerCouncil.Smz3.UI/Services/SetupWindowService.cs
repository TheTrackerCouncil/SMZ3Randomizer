using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media;
using AvaloniaControls.ControlServices;
using Material.Icons;
using SnesConnectorLibrary;
using SnesConnectorLibrary.Requests;
using SNI;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.UI.ViewModels;

namespace TrackerCouncil.Smz3.UI.Services;

public class SetupWindowService(OptionsFactory optionsFactory, ISnesConnectorService snesConnectorService) : ControlService
{
    private const string Z3Hash = "03a63945398191337e896e5771f77173";
    private const string SMHash = "21f3e98df4780ee1c667b84e57d88675";

    private SetupWindowViewModel _model = new();
    private RandomizerOptions _randomizerOptions = null!;
    private CancellationTokenSource _cancellationTokenSource = new();

    public SetupWindowViewModel GetViewModel()
    {
        snesConnectorService.GameDetected += SnesConnectorServiceOnConnected;
        _randomizerOptions = optionsFactory.Create();
        SetZeldaRomPath(_randomizerOptions.GeneralOptions.Z3RomPath);
        SetMetroidRomPath(_randomizerOptions.GeneralOptions.SMRomPath);
        _randomizerOptions.GeneralOptions.HasOpenedSetupWindow = true;
        _randomizerOptions.Save();
        return _model;
    }

    public bool SetRomPaths(IEnumerable<string> romPaths)
    {
        var anyInvalidRom = false;

        foreach (var path in romPaths)
        {
            if (CheckFileHash(path, Z3Hash))
            {
                _model.ZeldaRomPath = path;
                _model.IsValidZeldaRom = true;
            }
            else if (CheckFileHash(path, SMHash))
            {
                _model.MetroidRomPath = path;
                _model.IsValidMetroidRom = true;
            }
            else
            {
                anyInvalidRom = true;
            }
        }

        return !anyInvalidRom;
    }

    public async Task TestAutoTracking()
    {
        if (_model.IsConnecting)
        {
            return;
        }

        _model.IsConnecting = true;
        _cancellationTokenSource = new CancellationTokenSource();
        _model.AutoTrackerOpacity = 0.2f;
        _model.AutoTrackerIconKind = MaterialIconKind.CircleOutline;
        _model.AutoTrackerBrush = Brushes.White;
        _model.AutoTrackerMessage = "Connecting...";

        snesConnectorService.Connect(GetSnesConnectorSettings());

        try
        {
            await Task.Delay(TimeSpan.FromSeconds(20), _cancellationTokenSource.Token);

            // If the task wasn't cancelled, then it didn't connect at all fully
            _model.AutoTrackerOpacity = 1;
            _model.AutoTrackerIconKind = MaterialIconKind.CloseCircleOutline;
            _model.AutoTrackerBrush = Brushes.IndianRed;
            _model.AutoTrackerMessage = "Unable to connect";
            await _cancellationTokenSource.CancelAsync();
        }
        catch
        {
            // Do nothing
        }

        _model.IsConnecting = false;
        snesConnectorService.Disconnect();
    }

    public void SaveSettings()
    {
        if (_model.IsValidZeldaRom)
        {
            _randomizerOptions.GeneralOptions.Z3RomPath = _model.ZeldaRomPath;
        }

        if (_model.IsValidMetroidRom)
        {
            _randomizerOptions.GeneralOptions.SMRomPath = _model.MetroidRomPath;
        }

        _randomizerOptions.GeneralOptions.SnesConnectorSettings = GetSnesConnectorSettings();

        if (!_model.TrackerVoiceEnabled)
        {
            _randomizerOptions.GeneralOptions.SpeechRecognitionMode = SpeechRecognitionMode.Disabled;
            _randomizerOptions.GeneralOptions.TrackerVoiceFrequency = TrackerVoiceFrequency.Disabled;
        }

        _randomizerOptions.GeneralOptions.SelectedProfiles = GetSelectedProfiles();
        _randomizerOptions.Save();
    }

    public void OnClose()
    {
        snesConnectorService.GameDetected -= SnesConnectorServiceOnConnected;
        snesConnectorService.Disconnect();
    }

    private SnesConnectorSettings GetSnesConnectorSettings()
    {
        var snesConnectorSettings = new SnesConnectorSettings();

        if (_model.AutoTrackingDisable)
        {
            snesConnectorSettings.ConnectorType = SnesConnectorType.None;
        }
        else if (_model.AutoTrackingLua)
        {
            snesConnectorSettings.ConnectorType = SnesConnectorType.Lua;
        }
        else if (_model.AutoTrackingEmoTracker)
        {
            snesConnectorSettings.ConnectorType = SnesConnectorType.LuaEmoTracker;
        }
        else if (_model.AutoTrackingUsb2Snes)
        {
            snesConnectorSettings.ConnectorType = SnesConnectorType.Usb2Snes;
            snesConnectorSettings.Usb2SnesAddress = _model.ConnectorIpAddress;
        }
        else if (_model.AutoTrackingSni)
        {
            snesConnectorSettings.ConnectorType = SnesConnectorType.Sni;
            snesConnectorSettings.SniAddress = _model.ConnectorIpAddress;
        }

        return snesConnectorSettings;
    }

    private List<string?> GetSelectedProfiles()
    {
        var profiles = new List<string?>();

        if (_model.TrackerSassEnabled)
        {
            profiles.Add("Sassy");
        }

        if (_model.TrackerCursingEnabled)
        {
            profiles.Add("Rated T for Teen");
        }

        if (_model.TrackerBcuEnabled)
        {
            profiles.Add("BCU");
        }

        return profiles;
    }

    private void SetZeldaRomPath(string? path)
    {
        if (File.Exists(path) && CheckFileHash(path, Z3Hash))
        {
            _model.ZeldaRomPath = path;
            _model.IsValidZeldaRom = true;
        }
        else
        {
            _model.ZeldaRomPath = "Not Selected";
            _model.IsValidZeldaRom = false;
        }
    }

    private void SetMetroidRomPath(string? path)
    {
        if (File.Exists(path) && CheckFileHash(path, SMHash))
        {
            _model.MetroidRomPath = path;
            _model.IsValidMetroidRom = true;
        }
        else
        {
            _model.MetroidRomPath = "Not Selected";
            _model.IsValidMetroidRom = false;
        }
    }

    private bool CheckFileHash(string path, string expectedHash)
    {
        var bytes = File.ReadAllBytes(path);
        var hash = MD5.HashData(bytes);
        var hashString = BitConverter.ToString(hash).Replace("-", "");
        return expectedHash.Equals(hashString, StringComparison.OrdinalIgnoreCase);
    }

    private void SnesConnectorServiceOnConnected(object? sender, EventArgs e)
    {
        _ = TestGetMemoryAsync();
    }

    private async Task TestGetMemoryAsync()
    {
        for (var i = 0; i < 5 && !_cancellationTokenSource.IsCancellationRequested; i++)
        {
            var response = await snesConnectorService.MakeMemoryRequestAsync(new SnesSingleMemoryRequest()
            {
                MemoryRequestType = SnesMemoryRequestType.RetrieveMemory,
                SnesMemoryDomain = SnesMemoryDomain.ConsoleRAM,
                AddressFormat = AddressFormat.Snes9x,
                SniMemoryMapping = MemoryMapping.Unknown,
                Address = 0x7e0000,
                Length = 0x1
            });

            if (response is { Successful: true, HasData: true })
            {
                _model.AutoTrackerOpacity = 1;
                _model.AutoTrackerIconKind = MaterialIconKind.CheckCircleOutline;
                _model.AutoTrackerBrush = Brushes.Lime;
                _model.AutoTrackerMessage = "Connection successful!";
                break;
            }
            else
            {
                _model.AutoTrackerOpacity = 1;
                _model.AutoTrackerIconKind = MaterialIconKind.CloseCircleOutline;
                _model.AutoTrackerBrush = Brushes.IndianRed;
                _model.AutoTrackerMessage = "Invalid response";
                await Task.Delay(TimeSpan.FromSeconds(2f));
            }
        }

        await _cancellationTokenSource.CancelAsync();
    }
}
