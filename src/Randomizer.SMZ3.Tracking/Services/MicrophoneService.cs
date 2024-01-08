using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

using Microsoft.Extensions.Logging;

using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace Randomizer.SMZ3.Tracking.Services;

public class MicrophoneService : IMicrophoneService
{
    private readonly ManualResetEvent _manualResetEvent = new(false);
    private readonly ILogger<MicrophoneService> _logger;

    private MMDevice? _activeInputDevice;
    private IWaveIn? _activeCapture;
    private Stream? _stream;
    private bool _disposedValue;

    public MicrophoneService(ILogger<MicrophoneService> logger)
    {
        _logger = logger;
    }

    public string? DeviceName => _activeInputDevice?.DeviceFriendlyName;

    public string? DesiredAudioDevice { get; set; }

    public virtual bool CanRecord(out bool foundRequestedDevice)
    {
        var inputDevice = GetInputDevice(out foundRequestedDevice);
        return inputDevice != null;
    }

    public virtual void StartRecording(WaveFormat waveFormat)
    {
        _manualResetEvent.Reset();
        _activeInputDevice = GetInputDevice(out _);

        if (_activeInputDevice == null)
        {
            _logger.LogWarning("Invalid input device");
            return;
        }

        _logger.LogDebug("Starting recording using input device {Id} {Name}", _activeInputDevice.ID, _activeInputDevice.FriendlyName);

        _activeCapture = new WasapiCapture(_activeInputDevice)
        {
            WaveFormat = waveFormat,
            ShareMode = AudioClientShareMode.Shared,
        };
        _activeCapture.RecordingStopped += ActiveCapture_RecordingStopped;
        _activeCapture.DataAvailable += ActiveCapture_DataAvailable;

        _logger.LogDebug("Starting recording in {SampleRate} Hz, {BitsPerSample} bit, {Channels} channel(s)", _activeCapture.WaveFormat.SampleRate, _activeCapture.WaveFormat.BitsPerSample, _activeCapture.WaveFormat.Channels);
        _stream = new MemoryStream();
        _activeCapture.StartRecording();
    }

    public virtual Stream? StopRecording()
    {
        _logger.LogDebug("Stopping recording");
        _activeCapture?.StopRecording();
        _activeCapture?.Dispose();
        _activeCapture = null;

        _activeInputDevice?.Dispose();
        _activeInputDevice = null;

        _logger.LogDebug("Waiting for recording to stop");
        _manualResetEvent.WaitOne();

        _logger.LogDebug("Done");
        return _stream;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public Dictionary<string, string> GetDeviceDetails()
    {
        var toReturn = new Dictionary<string, string>();
        var enumerator = new MMDeviceEnumerator();
        foreach (var wasapi in enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active))
        {
            toReturn[wasapi.ID] = wasapi.DeviceFriendlyName;
        }

        return toReturn;
    }

    protected virtual MMDevice? GetInputDeviceById(string? id)
    {
        if (string.IsNullOrEmpty(id) || "Default" == id)
        {
            return WasapiCapture.GetDefaultCaptureDevice();
        }

        var enumerator = new MMDeviceEnumerator();
        return enumerator
            .EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active)
            .FirstOrDefault(wasapi => wasapi.ID == id);
    }

    protected virtual MMDevice? GetInputDevice(out bool foundRequestedDevice)
    {
        try
        {
            var device = GetInputDeviceById(DesiredAudioDevice);

            if (device == null)
            {
                _logger.LogWarning("Requested audio device {Id} not found", DesiredAudioDevice);
                foundRequestedDevice = false;
                return WasapiCapture.GetDefaultCaptureDevice();
            }

            foundRequestedDevice = true;
            return device;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Audio device not found");
            foundRequestedDevice = false;
            return null;
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _activeCapture?.Dispose();
                _activeInputDevice?.Dispose();
            }

            _activeCapture = null;
            _activeInputDevice = null;
            _disposedValue = true;
        }
    }

    private async void ActiveCapture_DataAvailable(object? sender, WaveInEventArgs e)
    {
        Debug.Assert(_stream != null, "Stream should be initialized before data becomes available");

        var buffer = e.Buffer.AsMemory(0, e.BytesRecorded);
        await _stream.WriteAsync(buffer);
    }

    private async void ActiveCapture_RecordingStopped(object? sender, StoppedEventArgs e)
    {
        if (_stream == null) return;

        await _stream.FlushAsync();
        _stream.Position = 0;
        _manualResetEvent.Set();

        _logger.LogDebug("Finished recording {Bytes} bytes", _stream.Length);
    }
}
