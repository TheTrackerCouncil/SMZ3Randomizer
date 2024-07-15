using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TrackerCouncil.Smz3.Chat.Integration;
using TrackerCouncil.Smz3.Chat.Integration.Models;
using TrackerCouncil.Smz3.Chat.Twitch.Models;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Exceptions;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;

namespace TrackerCouncil.Smz3.Chat.Twitch;

public class TwitchChatClient : IChatClient
{
    private readonly TwitchClient _twitchClient;
    private readonly IChatApi _chatApi;
    private bool _firstConnection = true;
    private readonly WebSocketClient TwitchWebSocketClient = new();

    public TwitchChatClient(ILogger<TwitchChatClient> logger, ILoggerFactory loggerFactory, IChatApi chatApi)
    {
        Logger = logger;

        _twitchClient = new TwitchClient(TwitchWebSocketClient, logger: loggerFactory.CreateLogger<TwitchClient>());

        _twitchClient.OnConnected += _twitchClient_OnConnected;
        _twitchClient.OnDisconnected += _twitchClient_OnDisconnected;
        _twitchClient.OnMessageReceived += _twitchClient_OnMessageReceived;
        _twitchClient.OnReconnected += _twitchClient_OnReconnected;
        _twitchClient.OnJoinedChannel += _twitchClient_OnJoinedChannel;
        _chatApi = chatApi;
    }

    public event EventHandler? Connected;

    public event EventHandler? Disconnected;

    public event EventHandler? Reconnected;

    public event EventHandler? SendMessageFailure;

    public event MessageReceivedEventHandler? MessageReceived;

    public string? ConnectedAs { get; protected set; }

    public string? Channel { get; protected set; }

    public string? OAuthToken { get; protected set; }

    public string? Id { get; protected set; }

    public bool IsConnected { get; protected set; }

    protected ILogger<TwitchChatClient> Logger { get; }

    public void Connect(string userName, string oauthToken, string channel, string id)
    {
        if (string.IsNullOrEmpty(_twitchClient.ConnectionCredentials?.TwitchUsername))
        {
            var credentials = new ConnectionCredentials(userName, oauthToken);
            _twitchClient.Initialize(credentials, channel);
        }

        _twitchClient.Connect();
        ConnectedAs = userName;
        Channel = channel;
        Id = userName.Equals(channel, StringComparison.OrdinalIgnoreCase) ? id : null;
        _chatApi.SetAccessToken(oauthToken);
    }

    public void Disconnect()
    {
        if (_twitchClient.IsInitialized)
        {
            IsConnected = false;
            _twitchClient.Disconnect();
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public Task SendMessageAsync(string message, bool announce = false)
    {
        if (announce)
        {
            return SendAnnouncementAsync(message);
        }

        try
        {
            _twitchClient.SendMessage(Channel, message);
        }
        catch (BadStateException e)
        {
            Logger.LogError(e, "Error in sending chat message");
            SendMessageFailure?.Invoke(this, new());
        }

        return Task.CompletedTask;
    }

    public async Task SendAnnouncementAsync(string message)
    {
        var announcement = new TwitchAnnouncement() { BroadcasterId = Id, ModeratorId = Id, Message = message };
        await _chatApi.MakeApiCallAsync<TwitchAnnouncement, TwitchAnnouncement>("chat/announcements", announcement, HttpMethod.Post, default);
    }

    protected virtual void OnConnected()
    {
        Connected?.Invoke(this, new());
    }

    protected virtual void OnReconnected()
    {
        Reconnected?.Invoke(this, new());
    }

    protected virtual void OnDisconnected()
    {
        Logger.LogWarning("Connection to chat lost");
        Disconnected?.Invoke(this, new());
    }

    protected virtual void OnMessageReceived(MessageReceivedEventArgs e)
    {
        MessageReceived?.Invoke(this, e);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            Disconnect();
            TwitchWebSocketClient.Dispose();
        }
    }

    private void _twitchClient_OnConnected(object? sender, TwitchLib.Client.Events.OnConnectedArgs e)
    {
        IsConnected = true;
    }

    private void _twitchClient_OnDisconnected(object? sender, TwitchLib.Communication.Events.OnDisconnectedEventArgs e)
    {
        if (IsConnected)
        {
            OnDisconnected();
        }
        IsConnected = false;
    }

    private void _twitchClient_OnMessageReceived(object? sender, TwitchLib.Client.Events.OnMessageReceivedArgs e)
    {
        OnMessageReceived(new MessageReceivedEventArgs(new TwitchChatMessage(e.ChatMessage)));
    }

    private void _twitchClient_OnReconnected(object? sender, TwitchLib.Communication.Events.OnReconnectedEventArgs e)
    {
        // Unfortunately this fires before reconnecting is genuinely finished, so we have to wait before
        // rejoining the original channel
        Task.Run(async () =>
        {
            await Task.Delay(TimeSpan.FromSeconds(3));
            _twitchClient.JoinChannel(Channel);
        });
    }

    private void _twitchClient_OnJoinedChannel(object? sender, OnJoinedChannelArgs e)
    {
        if (_firstConnection)
        {
            OnConnected();
            _firstConnection = false;
        }
        else
        {
            OnReconnected();
        }
    }

    public async Task<string?> CreatePollAsync(string title, ICollection<string> options, int duration)
    {
        // Create the poll object
        var poll = new TwitchPoll()
        {
            BroadcasterId = Id,
            Title = title,
            Choices = options.Select(x => new TwitchPollChoice()
            {
                Title = x
            }).ToList(),
            Duration = duration
        };

        var pollApiResponse = await _chatApi.MakeApiCallAsync<TwitchPoll, TwitchPoll>("polls", poll, HttpMethod.Post, default);

        return pollApiResponse != null && pollApiResponse.IsSuccessful ? pollApiResponse.Id : null;
    }

    public async Task<ChatPoll> CheckPollAsync(string id)
    {
        var pollApiResponse = await _chatApi.MakeApiCallAsync<TwitchPoll>($"polls?broadcaster_id={Id}&id={id}", HttpMethod.Get, default);

        if (pollApiResponse == null)
        {
            return new ChatPoll
            {
                IsPollComplete = true,
                IsPollSuccessful = false
            };
        }

        Logger.LogInformation("Poll complete with status {0} and winning choice of {1}", pollApiResponse.Status, pollApiResponse.WinningChoice?.Title);

        return new()
        {
            IsPollComplete = pollApiResponse.IsPollComplete,
            IsPollSuccessful = pollApiResponse.IsPollSuccessful,
            WinningChoice = pollApiResponse.WinningChoice?.Title
        };
    }

}
