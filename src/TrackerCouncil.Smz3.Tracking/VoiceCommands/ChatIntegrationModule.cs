﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Versioning;
using System.Speech.Recognition;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using PySpeechService.Recognition;
using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Chat.Integration;
using TrackerCouncil.Smz3.Chat.Integration.Models;
using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.Tracking.Services;

namespace TrackerCouncil.Smz3.Tracking.VoiceCommands;

/// <summary>
/// Provides Tracker with stream chat integration.
/// </summary>
public class ChatIntegrationModule : TrackerModule, IDisposable
{
    private static readonly Random s_random = new();
    private const string WinningGuessKey = "WinningGuess";
    private readonly Dictionary<string, int> _usersGreetedTimes = new();
    private readonly IPlayerProgressionService _playerProgressionService;
    private readonly ITrackerTimerService _timerService;
    private bool _askChatAboutContentCheckPollResults = true;
    private string? _askChatAboutContentPollId;
    private int _askChatAboutContentPollTime = 60;
    private bool _hasAskedChatAboutContent;
    private DateTimeOffset? _guessingGameStart;
    private DateTimeOffset? _guessingGameClosed ;
    private int? _trackerGuess;
    private bool waitingOnReconnect = false;

    /// <summary>
    /// Initializes a new instance of the <see
    /// cref="ChatIntegrationModule"/> class with the specified
    /// dependencies.
    /// </summary>
    /// <param name="tracker">The tracker instance to use.</param>
    /// <param name="playerProgressionService">Service to get item information</param>
    /// <param name="worldQueryService">Service to get world information</param>
    /// <param name="timerService"></param>
    /// <param name="chatClient">The chat client to use.</param>
    /// <param name="logger">Used to write logging information.</param>
    public ChatIntegrationModule(TrackerBase tracker, IChatClient chatClient, IPlayerProgressionService playerProgressionService, IWorldQueryService worldQueryService, ITrackerTimerService timerService, ILogger<ChatIntegrationModule> logger)
        : base(tracker, playerProgressionService, worldQueryService, logger)
    {
        ChatClient = chatClient;
        _playerProgressionService = playerProgressionService;
        _timerService = timerService;
        ChatClient.Connected += ChatClient_Connected;
        ChatClient.Reconnected += ChatClient_Reconnected;
        ChatClient.MessageReceived += ChatClient_MessageReceived;
        ChatClient.Disconnected += ChatClient_Disconnected;
        ChatClient.SendMessageFailure += ChatClient_SendMessageFailure;
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
            TrackerBase.Say(x => x.Chat.NoConnection);
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
        TrackerBase.Say(x => x.Chat.StartedGuessingGame);

        await Task.Delay(s_random.Next(100, 900));
        TrackerBase.Say(x => x.Chat.TrackerGuess, args: [_trackerGuess]);

        TrackerBase.AddUndo(() =>
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
            TrackerBase.Say(x => x.Chat.NoConnection);
            return;
        }

        var isModerator = !string.IsNullOrEmpty(moderator);

        if (_guessingGameStart == null)
        {
            TrackerBase.Say(x => isModerator
                ? x.Chat.ModeratorClosedGuessingGameBeforeStarting
                : x.Chat.ClosedGuessingGameBeforeStarting, args: [moderator]);
            return;
        }

        if (_guessingGameClosed != null)
        {
            var secondsSinceLastClose = (DateTimeOffset.Now - _guessingGameClosed.Value).TotalSeconds;
            if (secondsSinceLastClose > 10)
            {
                TrackerBase.Say(x => isModerator
                    ? x.Chat.ModeratorClosedGuessingGameWhileClosed
                    : x.Chat.ClosedGuessingGameWhileClosed, args: [moderator]);
            }
            return;
        }

        AllowGanonsTowerGuesses = false;
        _guessingGameClosed = DateTimeOffset.Now;

        if (!isModerator)
        {
            TrackerBase.Say(x => x.Chat.ClosedGuessingGame);

            var chatMessage = TrackerBase.Responses.Chat.ClosedGuessingGame?.ToString();
            if (chatMessage != null)
            {
                await ChatClient.SendMessageAsync(chatMessage, announce: true);
            }
        }
        else
        {
            TrackerBase.Say(x => x.Chat.ModeratorClosedGuessingGame, args: [moderator]);
        }

        TrackerBase.AddUndo(() =>
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
                TrackerBase.Say(x => x.Chat.NoConnection);
            return;
        }

        var currentValue = winningNumber;

        var winners = GanonsTowerGuesses
            .Where(x => x.Value == winningNumber)
            .Select(x => x.Key)
            .ToImmutableList();

        while (winners.Count == 0 && TrackerBase.Options.GanonsTowerGuessingGameStyle ==
               GanonsTowerGuessingGameStyle.ClosestWithoutGoingOver)
        {
            currentValue--;
            if (currentValue <= 0)
            {
                break;
            }

            winners = GanonsTowerGuesses
                .Where(x => x.Value == currentValue)
                .Select(x => x.Key)
                .ToImmutableList();
        }

        if (winners.Count == 0)
        {
            if (winningNumber == _trackerGuess)
            {
                TrackerBase.Say(x => x.Chat.TrackerGuessOnlyWinner, args: [winningNumber]);
            }
            else
            {
                TrackerBase.Say(x => x.Chat.NobodyWonGuessingGame, args: [winningNumber]);
            }

            var chatMessage = TrackerBase.Responses.Chat.NobodyWonGuessingGame?.Format(winningNumber);
            if (chatMessage != null)
            {
                await ChatClient.SendMessageAsync(chatMessage, announce: true);
            }
        }
        else
        {
            if (winningNumber == _trackerGuess)
                winners = winners.Add("Tracker");

            var pronouncedNames = winners.Select(TrackerBase.CorrectUserNamePronunciation);
            if (currentValue == winningNumber)
            {
                TrackerBase.Say(x => x.Chat.DeclareGuessingGameWinners, args: [winningNumber, NaturalLanguage.Join(pronouncedNames)]);
                var chatMessage = TrackerBase.Responses.Chat.DeclareGuessingGameWinners?.Format(winningNumber, NaturalLanguage.Join(winners));
                if (chatMessage != null)
                {
                    await ChatClient.SendMessageAsync(chatMessage, announce: true);
                }
            }
            else
            {
                TrackerBase.Say(x => x.Chat.DeclareGuessingGameClosestButNotOverWinner, args: [winningNumber, currentValue, NaturalLanguage.Join(pronouncedNames)]);
                var chatMessage = TrackerBase.Responses.Chat.DeclareGuessingGameClosestButNotOverWinner?.Format(winningNumber, currentValue, NaturalLanguage.Join(winners));
                if (chatMessage != null)
                {
                    await ChatClient.SendMessageAsync(chatMessage, announce: true);
                }
            }

            if (winningNumber == _trackerGuess)
            {
                await Task.Delay(s_random.Next(100, 900));
                TrackerBase.Say(x => x.Chat.TrackerGuessWon, args: [winningNumber]);
            }
        }

        if (winningNumber < _trackerGuess || (winningNumber != _trackerGuess && !isAutoTracked))
            TrackerBase.Say(x => x.Chat.TrackerGuessFailed, args: [winningNumber]);
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
            TrackerBase.Say(x => x.Chat.TrackerGuessFailed, args: [number]);
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
        var contentItemData = WorldQueryService.FirstOrDefault("Content");
        if (contentItemData == null)
        {
            Logger.LogError("Unable to determine content item data");
            TrackerBase.Say(x => x.Error);
            return;
        }

        // Always ask the first time, otherwise it's a random change. Can probably change to always be random later.
        var shouldAskChat = ChatClient.IsConnected && (!_hasAskedChatAboutContent || s_random.Next(0, 3) == 0);
        if (!ShouldCreatePolls || !shouldAskChat)
        {
            TrackerBase.ItemTracker.TrackItem(contentItemData);
            return;
        }

        _askChatAboutContentPollId = await ChatClient.CreatePollAsync("Do you think that was some high quality #content?", new List<string>() { "Yes", "No" }, _askChatAboutContentPollTime);

        if (string.IsNullOrEmpty(_askChatAboutContentPollId))
        {
            TrackerBase.ItemTracker.TrackItem(contentItemData);
            return;
        }

        TrackerBase.Say(x => x.Chat.AskChatAboutContent);
        TrackerBase.Say(x => x.Chat.PollOpened, args: [_askChatAboutContentPollTime]);
        _askChatAboutContentCheckPollResults = true;
        _hasAskedChatAboutContent = true;

        TrackerBase.AddUndo(() =>
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
                    TrackerBase.Say(x => result.WasPollTerminated ? x.Chat.PollCompleteTerminated : x.Chat.PollComplete);

                    if ("Yes".Equals(result.WinningChoice, StringComparison.OrdinalIgnoreCase))
                    {
                        TrackerBase.Say(x => x.Chat.AskChatAboutContentYes);
                        TrackerBase.ItemTracker.TrackItem(contentItemData);
                    }
                    else
                    {
                        TrackerBase.Say(x => x.Chat.AskChatAboutContentNo);
                    }
                }
                else
                {
                    TrackerBase.Say(x => result.WasPollTerminated ? x.Chat.PollErrorTerminated : x.Chat.PollError);
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
            var senderName = TrackerBase.CorrectUserNamePronunciation(e.Message.Sender);

            if (e.Message.SenderUserName.Equals("Dr_Dubz", StringComparison.OrdinalIgnoreCase))
                ProcessDrDubzChatMessage(e.Message, senderName);

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
            TrackerBase.Error();
        }
        finally
        {
            stopwatch.Stop();
            Logger.LogTrace("Processed incoming chat message in {ElapsedMs} ms", stopwatch.ElapsedMilliseconds);
        }
    }

    private bool ShouldRespondToGreetings => TrackerBase.Options.ChatGreetingEnabled
                                             && (TrackerBase.Options.ChatGreetingTimeLimit == 0
                                                 || _timerService.TotalElapsedTime.TotalMinutes <= TrackerBase.Options.ChatGreetingTimeLimit);

    private bool ShouldCreatePolls => TrackerBase.Options.PollCreationEnabled;

    private void TryRecordGanonsTowerGuess(ChatMessage message)
    {
        if (!AllowGanonsTowerGuesses)
            return;

        var validGuessPattern = new Regex("^(?<guess>(2[012]|1[0-9]|[0-9]))$",
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
        if (TrackerBase.Responses.Chat.RecognizedGreetings == null)
        {
            return;
        }

        foreach (var recognizedGreeting in TrackerBase.Responses.Chat.RecognizedGreetings)
        {
            if (Regex.IsMatch(message.Text, recognizedGreeting, RegexOptions.IgnoreCase | RegexOptions.Singleline))
            {
                // Sass if it was the broadcaster
                if (message.SenderUserName.Equals(TrackerBase.Options.UserName)
                    && TrackerBase.Responses.Chat.GreetedChannel != null)
                {
                    TrackerBase.Say(x => x.Chat.GreetedChannel, args: [senderNamePronunciation]);
                    break;
                }

                // Otherwise, keep track of the number of times someone said hi
                if (_usersGreetedTimes.TryGetValue(message.Sender, out var greeted))
                {
                    if (greeted >= 2)
                        break;

                    TrackerBase.Say(x => x.Chat.GreetedTwice, args: [senderNamePronunciation]);
                    _usersGreetedTimes[message.Sender]++;
                }
                else
                {
                    TrackerBase.Say(x => x.Chat.GreetingResponses, args: [senderNamePronunciation]);
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

    private void ProcessDrDubzChatMessage(ChatMessage message, string senderNamePronunciation)
    {
        var drDubzArtLinkPattern = new Regex("(https://(i\\.)?imgur\\.com/)", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(500));
        if (drDubzArtLinkPattern.IsMatch(message.Text))
        {
            TrackerBase.Say(x => x.Chat.DrDubzArtPosted);
        }
    }

    private void ChatClient_Connected(object? sender, EventArgs e)
    {
        TrackerBase.Say(x => x.Chat.WhenConnected);
    }

    private async void ChatClient_Disconnected(object? sender, EventArgs e)
    {
        try
        {
            waitingOnReconnect = true;
            await Task.Delay(TimeSpan.FromSeconds(15));
            if (waitingOnReconnect)
            {
                TrackerBase.Say(x => x.Chat.WhenDisconnected);
                waitingOnReconnect = false;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Unable to wait for reconnection");
        }
    }

    private void ChatClient_Reconnected(object? sender, EventArgs e)
    {
        if (waitingOnReconnect)
        {
            waitingOnReconnect = false;
        }
        else
        {
            TrackerBase.Say(x => x.Chat.WhenReconnected);
        }
    }

    private void ChatClient_SendMessageFailure(object? sender, EventArgs e)
    {
        TrackerBase.Error();
    }

    private SpeechRecognitionGrammarBuilder GetStartGuessingGameRule()
    {
        var commandRule = new SpeechRecognitionGrammarBuilder()
            .Append("Hey tracker, ")
            .OneOf("initiate", "start", "execute")
            .OneOf("Ganon's Tower Big Key Guessing Game",
                "GT Big Key Guessing Game",
                "Ganon's Tower Guessing Game",
                "GT Guessing Game",
                "order 66");

        var fromSpeech = new SpeechRecognitionGrammarBuilder()
            .Append("Hey tracker,")
            .OneOf("It's time for the GT big key guessing game",
                "The GT big key guessing game is now open for guesses");

        return SpeechRecognitionGrammarBuilder.Combine(commandRule, fromSpeech);
    }

    private SpeechRecognitionGrammarBuilder GetStopGuessingGameGuessesRule()
    {
        return new SpeechRecognitionGrammarBuilder()
            .Append("Hey tracker, ")
            .OneOf("close the floor",
                "close the floor for guesses",
                "the floor is now closed",
                "the floor is now closed for guesses");
    }

    private SpeechRecognitionGrammarBuilder GetRevealGuessingGameWinnerRule()
    {
        var validGuesses = new List<GrammarKeyValueChoice>();
        for (var i = 1; i <= 22; i++)
            validGuesses.Add(new GrammarKeyValueChoice(i.ToString(), i));

        return new SpeechRecognitionGrammarBuilder()
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

    private SpeechRecognitionGrammarBuilder GetTrackContent()
    {
        return new SpeechRecognitionGrammarBuilder()
            .Append("Hey tracker,")
            .Optional("please", "would you kindly")
            .OneOf("track", "add")
            .OneOf("content", "con-tent");
    }

    public override void AddCommands()
    {
        AddCommand("Start Ganon's Tower Big Key Guessing Game", GetStartGuessingGameRule(), async (result) =>
        {
            await StartGanonsTowerGuessingGame();
        });

        AddCommand("Close Ganon's Tower Big Key Guessing Game", GetStopGuessingGameGuessesRule(), async (result) =>
        {
            await CloseGanonsTowerGuessingGameGuesses();
        });

        AddCommand("Declare Ganon's Tower Big Key Guessing Game Winner", GetRevealGuessingGameWinnerRule(), async (result) =>
        {
            var winningNumber = int.Parse(result.Semantics[WinningGuessKey].Value);
            await DeclareGanonsTowerGuessingGameWinner(winningNumber);
        });

        AddCommand("Track Content", GetTrackContent(), async (result) =>
        {
            await AskChatAboutContent();
        });
    }
}
