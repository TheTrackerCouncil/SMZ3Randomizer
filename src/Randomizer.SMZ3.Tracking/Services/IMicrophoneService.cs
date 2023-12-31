using System;
using System.IO;

using NAudio.Wave;

namespace Randomizer.SMZ3.Tracking.Services;

public interface IMicrophoneService : IDisposable
{
    string? DeviceName { get; }

    bool CanRecord();

    void StartRecording(WaveFormat waveFormat);

    Stream? StopRecording();
}
