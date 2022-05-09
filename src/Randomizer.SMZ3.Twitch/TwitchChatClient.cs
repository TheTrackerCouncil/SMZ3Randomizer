using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Randomizer.SMZ3.ChatIntegration;
using Randomizer.SMZ3.ChatIntegration.Models;
using Randomizer.SMZ3.Twitch.Models;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace Randomizer.SMZ3.Twitch
{
    public class TwitchChatClient : IChatClient
    {
        private readonly TwitchClient _twitchClient;
        private readonly IChatApi _chatApi;

        public TwitchChatClient(ILogger<TwitchChatClient> logger, ILoggerFactory loggerFactory, IChatApi chatApi)
        {
            Logger = logger;

            _twitchClient = new(logger: loggerFactory.CreateLogger<TwitchClient>());
            _twitchClient.OnConnected += _twitchClient_OnConnected;
            _twitchClient.OnDisconnected += _twitchClient_OnDisconnected;
            _twitchClient.OnMessageReceived += _twitchClient_OnMessageReceived;
            _chatApi = chatApi;
        }

        public event EventHandler? Connected;

        public event MessageReceivedEventHandler? MessageReceived;

        public string? ConnectedAs { get; protected set; }

        public string? Channel { get; protected set; }

        public string? OAuthToken { get; protected set; }

        public string? Id { get; protected set; }

        public bool IsConnected { get; protected set; }

        protected ILogger<TwitchChatClient> Logger { get; }

        public void Connect(string userName, string oauthToken, string channel, string id)
        {
            if (!_twitchClient.IsInitialized)
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
                _twitchClient.Disconnect();
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
                message = $".announce {message}";
            }

            _twitchClient.SendMessage(Channel, message);
            return Task.CompletedTask;
        }

        protected virtual void OnConnected()
        {
            Connected?.Invoke(this, new());
        }

        protected virtual void OnMessageReceived(MessageReceivedEventArgs e)
        {
            MessageReceived?.Invoke(this, e);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_twitchClient.IsInitialized)
                    _twitchClient.Disconnect();
            }
        }

        private void _twitchClient_OnConnected(object? sender, TwitchLib.Client.Events.OnConnectedArgs e)
        {
            IsConnected = true;
            OnConnected();
        }

        private void _twitchClient_OnDisconnected(object? sender, TwitchLib.Communication.Events.OnDisconnectedEventArgs e)
        {
            IsConnected = false;
        }

        private void _twitchClient_OnMessageReceived(object? sender, TwitchLib.Client.Events.OnMessageReceivedArgs e)
        {
            OnMessageReceived(new MessageReceivedEventArgs(new TwitchChatMessage(e.ChatMessage)));
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

            poll = await _chatApi.MakeApiCallAsync<TwitchPoll, TwitchPoll>("polls", poll, HttpMethod.Post, default);

            return poll?.Id;
        }

        public async Task<ChatPoll> CheckPollAsync(string id)
        {
            var poll = await _chatApi.MakeApiCallAsync<TwitchPoll>($"polls?broadcaster_id={Id}&id={id}", HttpMethod.Get, default);

            if (poll == null)
            {
                return new ChatPoll
                {
                    IsComplete = true,
                    IsSuccessful = false
                };
            }

            Logger.LogInformation("Poll complete with status {0} and winning choice of {1}", poll.Status, poll.WinningChoice?.Title);

            return new()
            {
                IsComplete = poll.IsComplete,
                IsSuccessful = poll.Successful,
                WinningChoice = poll.WinningChoice?.Title
            };
        }

    }
}
