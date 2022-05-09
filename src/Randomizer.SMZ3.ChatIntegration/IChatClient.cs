using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Randomizer.SMZ3.ChatIntegration.Models;

namespace Randomizer.SMZ3.ChatIntegration
{
    public interface IChatClient : IDisposable
    {
        event EventHandler? Connected;

        event MessageReceivedEventHandler? MessageReceived;

        bool IsConnected { get; }
        string? ConnectedAs { get; }
        string? Channel { get; }

        void Connect(string userName, string oauthToken, string channel, string id);

        void Disconnect();

        Task SendMessageAsync(string message, bool announce = false);

        Task<string?> CreatePollAsync(string title, ICollection<string> options, int duration);

        Task<ChatPoll> CheckPollAsync(string id);
    }
}
