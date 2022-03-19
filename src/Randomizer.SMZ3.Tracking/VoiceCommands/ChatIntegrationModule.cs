using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Speech.Synthesis.TtsEngine;
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
        private readonly Dictionary<string, int> _usersGreetedTimes = new();

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
            var stopwatch = Stopwatch.StartNew();
            try
            {
                var userName = CorrectPronunciation(e.Message.Sender);

                if (ShouldRespondToGreetings)
                    TryRespondToGreetings(e.Message, userName);
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "An unexpected error occurred while processing incoming chat messages.");
                Tracker.Error();
            }
            finally
            {
                stopwatch.Stop();
                Logger.LogTrace("Processed incoming chat message in {ElapsedMs} ms", stopwatch.ElapsedMilliseconds);
            }
        }

        private bool ShouldRespondToGreetings => Tracker.Options.ChatGreetingEnabled
            && (Tracker.Options.ChatGreetingTimeLimit == 0
                || Tracker.TotalElapsedTime.TotalMinutes <= Tracker.Options.ChatGreetingTimeLimit);

        private string CorrectPronunciation(string userName)
        {
            if (Tracker.Responses.Chat.UserNamePronunciation.TryGetValue(userName, out var correctedUserName))
                return correctedUserName;
            return userName;
        }

        private void TryRespondToGreetings(ChatMessage message, string userName)
        {
            foreach (var recognizedGreeting in Tracker.Responses.Chat.RecognizedGreetings)
            {
                if (Regex.IsMatch(message.Text, recognizedGreeting, RegexOptions.IgnoreCase | RegexOptions.Singleline))
                {
                    if (_usersGreetedTimes.TryGetValue(message.Sender, out var greeted))
                    {
                        if (greeted >= 2)
                            break;

                        Tracker.Say(x => x.Chat.GreetedTwice, userName);
                        _usersGreetedTimes[message.Sender]++;
                    }
                    else
                    {
                        Tracker.Say(x => x.Chat.GreetingResponses, userName);
                        _usersGreetedTimes.Add(message.Sender, 1);
                    }
                    break;
                }
            }
        }

        private void ChatClient_Connected(object? sender, EventArgs e)
        {
            Tracker.Say(x => x.Chat.WhenConnected);
        }
    }
}
