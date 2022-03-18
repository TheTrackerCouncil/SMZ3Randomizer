using System;

namespace Randomizer.SMZ3.ChatIntegration
{
    public interface IChatClient : IDisposable
    {
        event EventHandler? Connected;

        event MessageReceivedEventHandler? MessageReceived;

        bool IsConnected { get; }
        string? ConnectedAs { get; }
        string? Channel { get; }

        void Connect(string userName, string oauthToken, string channel);

        void Disconnect();
    }
}
