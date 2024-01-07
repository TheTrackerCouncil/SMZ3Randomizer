using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using NAudio.Wave;

namespace Randomizer.SMZ3.Tracking.Services;

public interface IMicrophoneService : IDisposable
{
    string? DeviceName { get; }

    bool CanRecord();

    void StartRecording(WaveFormat waveFormat);

    Stream? StopRecording();

    ICollection<string> GetDeviceNames();

    string? DesiredAudioDevice { get; set; }
}
