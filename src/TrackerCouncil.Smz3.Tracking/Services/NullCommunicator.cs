using System;

namespace TrackerCouncil.Smz3.Tracking.Services;

internal class NullCommunicator : ICommunicator
{
    public void Dispose()
    {
        // Do nothing
    }

    public void Say(SpeechRequest request)
    {
        // Do nothing
    }

    public bool IsEnabled => false;

    public void Enable()
    {
        // Do nothing
    }

    public void Disable()
    {
        // Do nothing
    }

    public bool IsSpeaking => false;

    public void UpdateVolume(int volume)
    {
        // Do nothing
    }

#pragma warning disable CS0067
    public event EventHandler? SpeakStarted;
    public event EventHandler<SpeakCompletedEventArgs>? SpeakCompleted;
    public event EventHandler<SpeakingUpdatedEventArgs>? VisemeReached;
#pragma warning restore CS0067
}
