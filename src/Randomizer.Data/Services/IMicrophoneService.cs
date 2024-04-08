using System;
using System.Collections.Generic;
using System.IO;
using NAudio.Wave;

namespace Randomizer.Data.Services;

public interface IMicrophoneService : IDisposable
{
    string? DeviceName { get; }

    bool CanRecord(out bool foundRequestedDevice);

    void StartRecording(WaveFormat waveFormat);

    Stream? StopRecording();

    Dictionary<string, string> GetDeviceDetails();

    string? DesiredAudioDevice { get; set; }
}
