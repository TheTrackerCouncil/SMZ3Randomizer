using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TrackerCouncil.Smz3.Chat.Integration.Models;

namespace TrackerCouncil.Smz3.Chat.Integration;

public interface IChatClient : IAsyncDisposable, IDisposable
{
    event EventHandler? Connected;

    event EventHandler? Reconnected;

    event EventHandler? Disconnected;

    event EventHandler? SendMessageFailure;

    event MessageReceivedEventHandler? MessageReceived;

    bool IsConnected { get; }
    string? ConnectedAs { get; }
    string? Channel { get; }

    Task ConnectAsync(string userName, string oauthToken, string channel, string id);

    Task DisconnectAsync();

    Task SendMessageAsync(string message, bool announce = false);

    Task<string?> CreatePollAsync(string title, ICollection<string> options, int duration);

    Task<ChatPoll> CheckPollAsync(string id);
}
