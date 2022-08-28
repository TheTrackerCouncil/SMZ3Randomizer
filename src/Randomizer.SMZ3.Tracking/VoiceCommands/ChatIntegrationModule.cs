using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Speech.Recognition;
using System.Speech.Synthesis.TtsEngine;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Randomizer.SMZ3.ChatIntegration;
using Randomizer.SMZ3.ChatIntegration.Models;
using Randomizer.SMZ3.Tracking.Configuration;
using Randomizer.SMZ3.Tracking.Services;

namespace Randomizer.SMZ3.Tracking.VoiceCommands
{
    /// <summary>
    /// Provides Tracker with stream chat integration.
    /// </summary>
    public class ChatIntegrationModule : TrackerModule, IDisposable
    {
        private static readonly Random s_random = new();
        private const string WinningGuessKey = "WinningGuess";
        private readonly Dictionary<string, int> _usersGreetedTimes = new();
        private readonly IItemService _itemService;
        private bool _askChatAboutContentCheckPollResults = true;
        private string? _askChatAboutContentPollId;
        private int _askChatAboutContentPollTime = 60;
        private bool _hasAskedChatAboutContent = false;
        private DateTimeOffset? _guessingGameStart = null;
        private DateTimeOffset? _guessingGameClosed = null;
        private int? _trackerGuess = null;

        /// <summary>
        /// Initializes a new instance of the <see
        /// cref="ChatIntegrationModule"/> class with the specified
        /// dependencies.
        /// </summary>
        /// <param name="tracker">The tracker instance to use.</param>
        /// <param name="chatClient">The chat client to use.</param>
        /// <param name="logger">Used to write logging information.</param>
        public ChatIntegrationModule(Tracker tracker, IChatClient chatClient, IItemService itemService, ILogger<ChatIntegrationModule> logger)
            : base(tracker, itemService, logger)
        {
            ChatClient = chatClient;
            _itemService = itemService;
            ChatClient.Connected += ChatClient_Connected;
            ChatClient.MessageReceived += ChatClient_MessageReceived;
            ChatClient.Disconnected += ChatClient_Disconnected;
            ChatClient.SendMessageFailure += ChatClient_SendMessageFailure;

            AddCommand("Start Ganon's Tower Big Key Guessing Game", GetStartGuessingGameRule(), async (tracker, result) =>
            {
                await StartGanonsTowerGuessingGame();
            });

            AddCommand("Close Ganon's Tower Big Key Guessing Game", GetStopGuessingGameGuessesRule(), async (tracker, result) =>
            {
                await CloseGanonsTowerGuessingGameGuesses();
            });

            AddCommand("Declare Ganon's Tower Big Key Guessing Game Winner", GetRevealGuessingGameWinnerRule(), async (tracker, result) =>
            {
                var winningNumber = (int)result.Semantics[WinningGuessKey].Value;
                await DeclareGanonsTowerGuessingGameWinner(winningNumber);
            });

            AddCommand("Track Content", GetTrackContent(), async (tracker, result) =>
            {
                await AskChatAboutContent();
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
        public async Task StartGanonsTowerGuessingGame()
        {
            var lastCloseBackup = _guessingGameClosed;
            if (!ChatClient.IsConnected)
            {
                Tracker.Say(x => x.Chat.NoConnection);
                return;
            }

            if (!AllowGanonsTowerGuesses)
            {
                // Just in case this command gets misheard, only clear when not
                // already allowing guesses
                GanonsTowerGuesses.Clear();
            }
            AllowGanonsTowerGuesses = true;
            _guessingGameStart = DateTimeOffset.Now;
            _guessingGameClosed = null;
            _trackerGuess = s_random.Next(1, 23);
            Tracker.Say(x => x.Chat.StartedGuessingGame);

            await Task.Delay(s_random.Next(100, 900));
            Tracker.Say(x => x.Chat.TrackerGuess, _trackerGuess);

            Tracker.AddUndo(() =>
            {
                if (AllowGanonsTowerGuesses)
                {
                    AllowGanonsTowerGuesses = false;
                    GanonsTowerGuesses.Clear();
                    _guessingGameStart = null;
                    _guessingGameClosed = lastCloseBackup;
                    _trackerGuess = null;
                }
            });
        }

        /// <summary>
        /// Stops tracking GT Big Key Guessing Game guesses in chat.
        /// </summary>
        public async Task CloseGanonsTowerGuessingGameGuesses(string? moderator = null)
        {
            if (!ChatClient.IsConnected)
            {
                Tracker.Say(x => x.Chat.NoConnection);
                return;
            }

            var isModerator = !string.IsNullOrEmpty(moderator);

            if (_guessingGameStart == null)
            {
                Tracker.Say(x => isModerator
                    ? x.Chat.ModeratorClosedGuessingGameBeforeStarting
                    : x.Chat.ClosedGuessingGameBeforeStarting, moderator);
                return;
            }

            if (_guessingGameClosed != null)
            {
                var secondsSinceLastClose = (DateTimeOffset.Now - _guessingGameClosed.Value).TotalSeconds;
                if (secondsSinceLastClose > 10)
                {
                    Tracker.Say(x => isModerator
                        ? x.Chat.ModeratorClosedGuessingGameWhileClosed
                        : x.Chat.ClosedGuessingGameWhileClosed, moderator);
                }
                return;
            }

            AllowGanonsTowerGuesses = false;
            _guessingGameClosed = DateTimeOffset.Now;

            if (!isModerator)
            {
                Tracker.Say(x => x.Chat.ClosedGuessingGame);

                var chatMessage = Tracker.Responses.Chat.ClosedGuessingGame.ToString();
                if (chatMessage != null)
                {
                    await ChatClient.SendMessageAsync(chatMessage, announce: true);
                }
            }
            else
            {
                Tracker.Say(x => x.Chat.ModeratorClosedGuessingGame, moderator);
            }

            Tracker.AddUndo(() =>
            {
                if (!AllowGanonsTowerGuesses)
                {
                    AllowGanonsTowerGuesses = true;
                    _guessingGameClosed = null;
                }
            });
        }

        /// <summary>
        /// Reveals the winner of the Ganon's Tower Big Key Guessing Game.
        /// </summary>
        /// <param name="winningNumber">The correct number.</param>
        /// <param name="isAutoTracked">If declared via auto tracker</param>
        public async Task DeclareGanonsTowerGuessingGameWinner(int winningNumber, bool isAutoTracked = false)
        {
            if (!ChatClient.IsConnected)
            {
                if (!isAutoTracked)
                    Tracker.Say(x => x.Chat.NoConnection);
                return;
            }

            var winners = GanonsTowerGuesses
                .Where(x => x.Value == winningNumber)
                .Select(x => x.Key)
                .ToImmutableList();

            if (winners.Count == 0)
            {
                if (winningNumber == _trackerGuess)
                {
                    Tracker.Say(x => x.Chat.TrackerGuessOnlyWinner, winningNumber);
                }
                else
                {
                    Tracker.Say(x => x.Chat.NobodyWonGuessingGame, winningNumber);
                }

                var chatMessage = Tracker.Responses.Chat.NobodyWonGuessingGame.Format(winningNumber);
                if (chatMessage != null)
                {
                    await ChatClient.SendMessageAsync(chatMessage, announce: true);
                }
            }
            else
            {
                if (winningNumber == _trackerGuess)
                    winners = winners.Add("Tracker");

                var pronouncedNames = winners.Select(Tracker.CorrectUserNamePronunciation);
                Tracker.Say(x => x.Chat.DeclareGuessingGameWinners, winningNumber, NaturalLanguage.Join(pronouncedNames));

                var chatMessage = Tracker.Responses.Chat.DeclareGuessingGameWinners.Format(winningNumber, NaturalLanguage.Join(winners));
                if (chatMessage != null)
                {
                    await ChatClient.SendMessageAsync(chatMessage, announce: true);
                }

                if (winningNumber == _trackerGuess)
                {
                    await Task.Delay(s_random.Next(100, 900));
                    Tracker.Say(x => x.Chat.TrackerGuessWon, winningNumber);
                }
            }

            if (winningNumber < _trackerGuess || (winningNumber != _trackerGuess && !isAutoTracked))
                Tracker.Say(x => x.Chat.TrackerGuessFailed, winningNumber);
        }

        /// <summary>
        /// Method to call when the player sees or picks up a GT item
        /// </summary>
        /// <param name="number">The item number</param>
        /// <param name="wasBigKey">If the the item was the big key or not</param>
        public async Task GTItemTracked(int number, bool wasBigKey)
        {
            if (!_guessingGameStart.HasValue) return;
            if (!_guessingGameClosed.HasValue)
                _ = CloseGanonsTowerGuessingGameGuesses();

            if (!wasBigKey && number == _trackerGuess)
            {
                Tracker.Say(x => x.Chat.TrackerGuessFailed, number);
            }
            else if (wasBigKey)
            {
                await DeclareGanonsTowerGuessingGameWinner(number, true);
            }
        }

        /// <summary>
        /// Asks the chat if tracker should increase content by one step
        /// </summary>
        /// <returns></returns>
        public async Task AskChatAboutContent()
        {
            var contentItemData = _itemService.FindOrDefault("Content");
            if (contentItemData == null)
            {
                Logger.LogError("Unable to determine content item data");
                Tracker.Say(x => x.Error);
                return;
            }

            // Always ask the first time, otherwise it's a random change. Can probably change to always be random later.
            var shouldAskChat = ChatClient.IsConnected && (!_hasAskedChatAboutContent || s_random.Next(0, 3) == 0);
            if (!ShouldCreatePolls || !shouldAskChat)
            {
                Tracker.TrackItem(contentItemData);
                return;
            }

            _askChatAboutContentPollId = await ChatClient.CreatePollAsync("Do you think that was some high quality #content?", new List<string>() { "Yes", "No" }, _askChatAboutContentPollTime);

            if (string.IsNullOrEmpty(_askChatAboutContentPollId))
            {
                Tracker.TrackItem(contentItemData);
                return;
            }

            Tracker.Say(x => x.Chat.AskChatAboutContent);
            Tracker.Say(x => x.Chat.PollOpened, _askChatAboutContentPollTime);
            _askChatAboutContentCheckPollResults = true;
            _hasAskedChatAboutContent = true;

            Tracker.AddUndo(() =>
            {
                _askChatAboutContentCheckPollResults = false;
            });

            await Task.Delay(TimeSpan.FromSeconds(_askChatAboutContentPollTime + 5));

            do
            {
                var result = await ChatClient.CheckPollAsync(_askChatAboutContentPollId);
                if (result.IsPollComplete && _askChatAboutContentCheckPollResults)
                {
                    _askChatAboutContentCheckPollResults = false;

                    if (result.IsPollSuccessful)
                    {
                        Tracker.Say(x => x.Chat.PollComplete);

                        if ("Yes".Equals(result.WinningChoice, StringComparison.OrdinalIgnoreCase))
                        {
                            Tracker.Say(x => x.Chat.AskChatAboutContentYes);
                            Tracker.TrackItem(contentItemData);
                        }
                        else
                        {
                            Tracker.Say(x => x.Chat.AskChatAboutContentNo);
                        }
                    }
                    else
                    {
                        Tracker.Say(x => x.Chat.PollError);
                    }
                }
                else if (_askChatAboutContentCheckPollResults)
                {
                    await Task.Delay(TimeSpan.FromSeconds(5));
                }
            } while (_askChatAboutContentCheckPollResults);

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

        private bool ShouldCreatePolls => Tracker.Options.PollCreationEnabled;

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
                var _ = CloseGanonsTowerGuessingGameGuesses(senderNamePronunciation);
            }
        }

        private void ChatClient_Connected(object? sender, EventArgs e)
        {
            Tracker.Say(x => x.Chat.WhenConnected);
        }

        private void ChatClient_Disconnected(object? sender, EventArgs e)
        {
            Tracker.Say(x => x.Chat.WhenDisconnected);
        }

        private void ChatClient_SendMessageFailure(object? sender, EventArgs e)
        {
            Tracker.Error();
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
                .Append("Hey tracker,")
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

        private GrammarBuilder GetTrackContent()
        {
            return new GrammarBuilder()
                .Append("Hey tracker,")
                .Optional("please", "would you kindly")
                .OneOf("track", "add")
                .OneOf("content", "con-tent");
        }
    }
}
