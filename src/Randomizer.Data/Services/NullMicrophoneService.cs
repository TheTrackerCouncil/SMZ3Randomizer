using System;
using System.Collections.Generic;
using System.IO;
using NAudio.Wave;

namespace Randomizer.Data.Services;

public class NullMicrophoneService : IMicrophoneService
{

    public string? DeviceName => "";

    public bool CanRecord(out bool foundRequestedDevice)
    {
        foundRequestedDevice = false;
        return false;
    }

    public void StartRecording(WaveFormat waveFormat)
    {
        // Do nothing
    }

    public Stream? StopRecording() => null;

    public Dictionary<string, string> GetDeviceDetails() => new();

    public string? DesiredAudioDevice { get; set; } = "";

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
