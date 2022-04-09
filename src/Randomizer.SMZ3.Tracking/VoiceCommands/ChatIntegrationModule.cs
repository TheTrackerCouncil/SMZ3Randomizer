using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Speech.Recognition;
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
        private const string WinningGuessKey = "WinningGuess";
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

            AddCommand("Start Ganon's Tower Big Key Guessing Game", GetStartGuessingGameRule(), (tracker, result) =>
            {
                StartGanonsTowerGuessingGame();
            });

            AddCommand("Close Ganon's Tower Big Key Guessing Game", GetStopGuessingGameGuessesRule(), (tracker, result) =>
            {
                CloseGanonsTowerGuessingGameGuesses();
            });

            AddCommand("Declare Ganon's Tower Big Key Guessing Game Winner", GetRevealGuessingGameWinnerRule(), (tracker, result) =>
            {
                var winningNumber = (int)result.Semantics[WinningGuessKey].Value;
                DeclareGanonsTowerGuessingGameWinner(winningNumber);
            });
        }

        /// <summary>
        /// Gets or sets a value indicating whether Tracker will accept guesses
        /// for the Ganon's Tower Big Key Guessing Game.
        /// </summary>
        public bool AllowGanonsTowerGuesses { get; set; }

        /// <summary>
        /// Gets a dictionary containing the users who participated in the GT
        /// Big Key Guessing Game and their guess.
        /// </summary>
        public Dictionary<string, int> GanonsTowerGuesses { get; } = new();

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
        /// Starts tracking GT Big Key Guessing Game guesses in chat.
        /// </summary>
        public void StartGanonsTowerGuessingGame()
        {
            GanonsTowerGuesses.Clear();
            AllowGanonsTowerGuesses = true;
            Tracker.Say(x => x.Chat.StartedGuessingGame);
        }

        /// <summary>
        /// Stops tracking GT Big Key Guessing Game guesses in chat.
        /// </summary>
        public void CloseGanonsTowerGuessingGameGuesses(string? moderator = null)
        {
            AllowGanonsTowerGuesses = false;

            if (string.IsNullOrEmpty(moderator))
            {
                Tracker.Say(x => x.Chat.ClosedGuessingGame);
            }
            else
            {
                Tracker.Say(x => x.Chat.ModeratorClosedGuessingGame, moderator);
            }
        }

        /// <summary>
        /// Reveals the winner of the Ganon's Tower Big Key Guessing Game.
        /// </summary>
        /// <param name="winningNumber">The correct number.</param>
        public void DeclareGanonsTowerGuessingGameWinner(int winningNumber)
        {
            var winners = GanonsTowerGuesses
                .Where(x => x.Value == winningNumber)
                .Select(x => x.Key)
                .ToImmutableList();

            if (winners.Count == 0)
            {
                Tracker.Say(x => x.Chat.NobodyWonGuessingGame, winningNumber);
            }
            else
            {
                var names = winners.Select(Tracker.CorrectUserNamePronunciation);
                Tracker.Say(x => x.Chat.DeclareGuessingGameWinners, winningNumber, NaturalLanguage.Join(names));
            }
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
                var senderName = Tracker.CorrectUserNamePronunciation(e.Message.SenderUserName);

                if (ShouldRespondToGreetings)
                    TryRespondToGreetings(e.Message, senderName);

                if (AllowGanonsTowerGuesses)
                    TryRecordGanonsTowerGuess(e.Message);

                if (e.Message.IsFromModerator)
                    ProcessModChatCommand(e.Message, senderName);
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

        private void TryRecordGanonsTowerGuess(ChatMessage message)
        {
            if (!AllowGanonsTowerGuesses)
                return;

            var validGuessPattern = new Regex("\\b(?<guess>(2[012]|1[0-9]|[0-9]))\\b",
                RegexOptions.ExplicitCapture, TimeSpan.FromMilliseconds(200));
            var match = validGuessPattern.Match(message.Text);
            if (match.Success)
            {
                var guess = match.Groups["guess"].Value;
                if (int.TryParse(guess, out var value))
                {
                    GanonsTowerGuesses[message.Sender] = value;
                }
            }
        }

        private void TryRespondToGreetings(ChatMessage message, string senderNamePronunciation)
        {
            foreach (var recognizedGreeting in Tracker.Responses.Chat.RecognizedGreetings)
            {
                if (Regex.IsMatch(message.Text, recognizedGreeting, RegexOptions.IgnoreCase | RegexOptions.Singleline))
                {
                    // Sass if it was the broadcaster
                    if (message.SenderUserName.Equals(Tracker.Options.UserName)
                        && Tracker.Responses.Chat.GreetedChannel != null)
                    {
                        Tracker.Say(x => x.Chat.GreetedChannel, senderNamePronunciation);
                        break;
                    }

                    // Otherwise, keep track of the number of times someone said hi
                    if (_usersGreetedTimes.TryGetValue(message.Sender, out var greeted))
                    {
                        if (greeted >= 2)
                            break;

                        Tracker.Say(x => x.Chat.GreetedTwice, senderNamePronunciation);
                        _usersGreetedTimes[message.Sender]++;
                    }
                    else
                    {
                        Tracker.Say(x => x.Chat.GreetingResponses, senderNamePronunciation);
                        _usersGreetedTimes.Add(message.Sender, 1);
                    }
                    break;
                }
            }
        }

        private void ProcessModChatCommand(ChatMessage message, string senderNamePronunciation)
        {
            if (!message.IsFromModerator)
                return;

            var closeGuessesPattern = new Regex("^(Hey tracker, )?(close( the floor( for guesses)?)?|the floor is (now )?closed( for guesses)?)$",
                RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(500));
            if (closeGuessesPattern.IsMatch(message.Text))
            {
                CloseGanonsTowerGuessingGameGuesses(senderNamePronunciation);
            }
        }

        private void ChatClient_Connected(object? sender, EventArgs e)
        {
            Tracker.Say(x => x.Chat.WhenConnected);
        }

        private GrammarBuilder GetStartGuessingGameRule()
        {
            var commandRule = new GrammarBuilder()
                .Append("Hey tracker, ")
                .OneOf("initiate", "start", "execute")
                .OneOf("Ganon's Tower Big Key Guessing Game",
                    "GT Big Key Guessing Game",
                    "Ganon's Tower Guessing Game",
                    "GT Guessing Game",
                    "order 66");

            var fromSpeech = new GrammarBuilder()
                // .Append("Hey tracker,") // Re-add this if this is causing too many matches
                .OneOf("It's time for the GT big key guessing game",
                    "The GT big key guessing game is now open for guesses");

            return GrammarBuilder.Combine(commandRule, fromSpeech);
        }

        private GrammarBuilder GetStopGuessingGameGuessesRule()
        {
            return new GrammarBuilder()
                .Append("Hey tracker, ")
                .OneOf("close the floor",
                    "close the floor for guesses",
                    "the floor is now closed",
                    "the floor is now closed for guesses");
        }

        private GrammarBuilder GetRevealGuessingGameWinnerRule()
        {
            var validGuesses = new Choices();
            for (var i = 1; i <= 22; i++)
                validGuesses.Add(new SemanticResultValue(i.ToString(), i));

            return new GrammarBuilder()
                .Append("Hey tracker, ")
                .OneOf("who guessed",
                    "who guessed number",
                    "the big key was in chest number",
                    "the big key was in chest",
                    "the lucky number is",
                    "the winning number is",
                    "the correct number is")
                .Append(WinningGuessKey, validGuesses);
        }
    }
}
