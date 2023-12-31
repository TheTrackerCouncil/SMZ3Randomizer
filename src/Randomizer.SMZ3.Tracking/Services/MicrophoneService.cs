using System;
using System.Diagnostics;
using System.IO;
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

    public virtual bool CanRecord()
    {
        var inputDevice = GetInputDevice();
        return inputDevice != null;
    }

    public virtual void StartRecording(WaveFormat waveFormat)
    {
        _manualResetEvent.Reset();
        _activeInputDevice = GetInputDevice();
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

    protected virtual MMDevice GetInputDevice()
    {
        // TODO: Get requested input device from settings, or fall back to default
        return WasapiCapture.GetDefaultCaptureDevice();
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
