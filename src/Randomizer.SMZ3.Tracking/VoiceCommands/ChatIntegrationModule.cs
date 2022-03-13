using System;
using System.Text.RegularExpressions;

using Microsoft.Extensions.Logging;

using Randomizer.SMZ3.ChatIntegration;

namespace Randomizer.SMZ3.Tracking.VoiceCommands
{
    /// <summary>
    /// Provides Tracker with stream chat integration.
    /// </summary>
    public class ChatIntegrationModule : TrackerModule, IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see
        /// cref="ChatIntegrationModule"/> class with the specified
        /// dependencies.
        /// </summary>
        /// <param name="tracker">The tracker instance to use.</param>
        /// <param name="chatClient">The chat client to use.</param>
        /// <param name="logger">Used to write logging information.</param>
        public ChatIntegrationModule(Tracker tracker, IChatClient chatClient, ILogger<ChatIntegrationModule> logger)
            : base(tracker, logger)
        {
            ChatClient = chatClient;
            ChatClient.Connected += ChatClient_Connected;
            ChatClient.MessageReceived += ChatClient_MessageReceived;
        }

        /// <summary>
        /// Gets the client used to integrate with chat.
        /// </summary>
        protected IChatClient ChatClient { get; }

        /// <summary>
        /// Frees up resources used by the <see cref="ChatIntegrationModule"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Frees up resources used by the <see cref="ChatIntegrationModule"/>.
        /// </summary>
        /// <param name="disposing">
        /// <see langword="true"/> when called from <see cref="Dispose()"/>;
        /// otherwise, <see langword="false"/>.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                ChatClient.Connected -= ChatClient_Connected;
                ChatClient.MessageReceived -= ChatClient_MessageReceived;
            }
        }

        private void ChatClient_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            foreach (var recognizedGreeting in Tracker.Responses.Chat.RecognizedGreetings)
            {
                if (Regex.IsMatch(e.Message.Text, recognizedGreeting, RegexOptions.IgnoreCase | RegexOptions.Singleline))
                {
                    var userName = e.Message.Sender;
                    Tracker.Responses.Chat.UserNamePronunciation.TryGetValue(userName, out userName);

                    Tracker.Say(x => x.Chat.GreetingResponses, userName);
                    return;
                }
            }
        }

        private void ChatClient_Connected(object? sender, EventArgs e)
        {
            Tracker.Say(x => x.Chat.WhenConnected);
        }
    }
}
