using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Randomizer.SMZ3.ChatIntegration;

using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace Randomizer.SMZ3.Twitch
{
    public class TwitchChatClient : IChatClient
    {
        private readonly TwitchClient _twitchClient;

        public TwitchChatClient(ILogger<TwitchChatClient> logger, ILoggerFactory loggerFactory)
        {
            Logger = logger;

            _twitchClient = new(logger: loggerFactory.CreateLogger<TwitchClient>());
            _twitchClient.OnConnected += _twitchClient_OnConnected;
            _twitchClient.OnDisconnected += _twitchClient_OnDisconnected;
            _twitchClient.OnMessageReceived += _twitchClient_OnMessageReceived;
        }

        public event EventHandler? Connected;

        public event MessageReceivedEventHandler? MessageReceived;

        public string? ConnectedAs { get; protected set; }

        public string? Channel { get; protected set; }

        public bool IsConnected { get; protected set; }

        protected ILogger<TwitchChatClient> Logger { get; }

        public void Connect(string userName, string oauthToken, string channel)
        {
            if (!_twitchClient.IsInitialized)
            {
                var credentials = new ConnectionCredentials(userName, oauthToken);
                _twitchClient.Initialize(credentials, channel);
            }

            _twitchClient.Connect();
            ConnectedAs = userName;
            Channel = channel;
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
    }
}
