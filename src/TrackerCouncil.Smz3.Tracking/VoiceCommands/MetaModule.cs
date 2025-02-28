using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using PySpeechServiceClient.Grammar;
using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Tracking.Services;

namespace TrackerCouncil.Smz3.Tracking.VoiceCommands;

/// <summary>
/// Provides voice commands for interacting with Tracker itself.
/// </summary>
public class MetaModule : TrackerModule
{
    private readonly ICommunicator _communicator;
    private const string ModifierKey = "Increase/Decrease";
    private const string ThresholdSettingKey = "ThresholdSetting";
    private const string ValueKey = "Value";
    private const int ThresholdSetting_Recognition = 0;
    private const int ThresholdSetting_Execution = 1;
    private const int ThresholdSetting_Sass = 2;

    /// <summary>
    /// Initializes a new instance of the <see cref="MetaModule"/> class.
    /// </summary>
    /// <param name="tracker">The tracker instance.</param>
    /// <param name="playerProgressionService">Service to get item information</param>
    /// <param name="worldQueryService">Service to get world information</param>
    /// <param name="logger">Used to write logging information.</param>
    /// <param name="communicator">Used to communicate information to the user</param>
    public MetaModule(TrackerBase tracker, IPlayerProgressionService playerProgressionService, IWorldQueryService worldQueryService, ILogger<MetaModule> logger, ICommunicator communicator)
        : base(tracker, playerProgressionService, worldQueryService, logger)
    {
        _communicator = communicator;
    }

    [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
    private static List<GrammarKeyValueChoice> GetIncreaseDecrease()
    {
        var modifiers = new List<GrammarKeyValueChoice>();
        modifiers.Add(new GrammarKeyValueChoice("increase", 1));
        modifiers.Add(new GrammarKeyValueChoice("decrease", -1));
        return modifiers;
    }

    [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
    private static List<GrammarKeyValueChoice> GetOneThroughTenPercent()
    {
        var values = new List<GrammarKeyValueChoice>();
        values.Add(new GrammarKeyValueChoice("one percent", 0.01));
        values.Add(new GrammarKeyValueChoice("two percent", 0.02));
        values.Add(new GrammarKeyValueChoice("three percent", 0.03));
        values.Add(new GrammarKeyValueChoice("four percent", 0.04));
        values.Add(new GrammarKeyValueChoice("five percent", 0.05));
        values.Add(new GrammarKeyValueChoice("six percent", 0.06));
        values.Add(new GrammarKeyValueChoice("seven percent", 0.07));
        values.Add(new GrammarKeyValueChoice("eight percent", 0.08));
        values.Add(new GrammarKeyValueChoice("nine percent", 0.09));
        values.Add(new GrammarKeyValueChoice("ten percent", 0.10));
        return values;
    }

    [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
    private static List<GrammarKeyValueChoice> GetThresholdSettings()
    {
        var settings = new List<GrammarKeyValueChoice>();
        settings.Add(new GrammarKeyValueChoice("recognition", ThresholdSetting_Recognition));
        settings.Add(new GrammarKeyValueChoice("execution", ThresholdSetting_Execution));
        settings.Add(new GrammarKeyValueChoice("sass", ThresholdSetting_Sass));
        return settings;
    }

    private SpeechRecognitionGrammarBuilder GetRepeatThatRule()
    {
        return new SpeechRecognitionGrammarBuilder()
            .Append("Hey tracker, ")
            .OneOf("can you repeat that?",
                "can you please repeat that?",
                "what was that?",
                "repeat that",
                "please repeat that");
    }

    private SpeechRecognitionGrammarBuilder GetShutUpRule()
    {
        return new SpeechRecognitionGrammarBuilder()
            .Append("Hey tracker, ")
            .Optional("please")
            .OneOf("shut up", "be quiet", "stop talking");
    }

    private SpeechRecognitionGrammarBuilder GetIncreaseThresholdGrammar()
    {
        var modifiers = GetIncreaseDecrease();
        var settings = GetThresholdSettings();
        var values = GetOneThroughTenPercent();
        return new SpeechRecognitionGrammarBuilder()
            .Append("Hey tracker,")
            .Append(ModifierKey, modifiers)
            .Optional("minimum")
            .Append(ThresholdSettingKey, settings)
            .OneOf("threshold", "confidence")
            .Append("by")
            .Append(ValueKey, values);
    }

    private SpeechRecognitionGrammarBuilder GetPauseTimerRule()
    {
        return new SpeechRecognitionGrammarBuilder()
            .Append("Hey tracker, ")
            .Optional("please")
            .OneOf("pause the timer", "stop the timer");
    }

    private SpeechRecognitionGrammarBuilder GetResumeTimerRule()
    {
        return new SpeechRecognitionGrammarBuilder()
            .Append("Hey tracker, ")
            .Optional("please")
            .OneOf("resume the timer", "start the timer");
    }

    private SpeechRecognitionGrammarBuilder GetResetTimerRule()
    {
        return new SpeechRecognitionGrammarBuilder()
            .Append("Hey tracker, ")
            .Optional("please")
            .OneOf("reset the timer");
    }

    private SpeechRecognitionGrammarBuilder GetMuteRule()
    {
        return new SpeechRecognitionGrammarBuilder()
            .Append("Hey tracker, ")
            .Optional("please")
            .OneOf("mute yourself", "silence yourself");
    }

    private SpeechRecognitionGrammarBuilder GetUnmuteRule()
    {
        return new SpeechRecognitionGrammarBuilder()
            .Append("Hey tracker, ")
            .Optional("please")
            .OneOf("unmute yourself", "unsilence yourself");
    }

    private SpeechRecognitionGrammarBuilder GetBeatGameRule()
    {
        return new SpeechRecognitionGrammarBuilder()
            .Append("Hey tracker, ")
            .OneOf("I beat", "I finished")
            .OneOf("the game", "the seed");
    }

    [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
    public override void AddCommands()
    {
        AddCommand("Repeat that", GetRepeatThatRule(), (_) =>
        {
            TrackerBase.Repeat();
        });

        AddCommand("Shut up", GetShutUpRule(), (_) =>
        {
            TrackerBase.ShutUp();
        });

        AddCommand("Temporarily change threshold setting", GetIncreaseThresholdGrammar(), (result) =>
        {
            var modifier = int.Parse(result.Semantics[ModifierKey].Value);
            var thresholdSetting = int.Parse(result.Semantics[ThresholdSettingKey].Value);
            var value = float.Parse(result.Semantics[ValueKey].Value);

            var adjustment = modifier * value;
            switch (thresholdSetting)
            {
                case ThresholdSetting_Recognition:
                    TrackerBase.Options.MinimumRecognitionConfidence += adjustment;
                    TrackerBase.Say(text: TrackerBase.Responses.TrackerSettingChanged?.Format(
                        "recognition threshold", $"{TrackerBase.Options.MinimumRecognitionConfidence:P0}"));
                    Logger.LogInformation("Temporarily changed recognition threshold to {newValue}", TrackerBase.Options.MinimumRecognitionConfidence);
                    break;

                case ThresholdSetting_Execution:
                    TrackerBase.Options.MinimumExecutionConfidence += adjustment;
                    TrackerBase.Say(text: TrackerBase.Responses.TrackerSettingChanged?.Format(
                        "execution threshold", $"{TrackerBase.Options.MinimumExecutionConfidence:P0}"));
                    Logger.LogInformation("Temporarily changed execution threshold to {newValue}", TrackerBase.Options.MinimumExecutionConfidence);
                    break;

                case ThresholdSetting_Sass:
                    TrackerBase.Options.MinimumSassConfidence += adjustment;
                    TrackerBase.Say(text: TrackerBase.Responses.TrackerSettingChanged?.Format(
                        "sass threshold", $"{TrackerBase.Options.MinimumSassConfidence:P0}"));
                    Logger.LogInformation("Temporarily changed sass threshold to {newValue}", TrackerBase.Options.MinimumSassConfidence);
                    break;

                default:
                    throw new ArgumentException($"The threshold setting '{thresholdSetting}' was not recognized.");
            }
        });

        if (TrackerBase.Options.TrackerTimerEnabled)
        {
            AddCommand("Pause timer", GetPauseTimerRule(), (_) =>
            {
                TrackerBase.PauseTimer();
            });

            AddCommand("Start timer", GetResumeTimerRule(), (_) =>
            {
                TrackerBase.StartTimer();
            });

            AddCommand("Reset timer", GetResetTimerRule(), (_) =>
            {
                TrackerBase.ResetTimer();
            });
        }

        AddCommand("Mute", GetMuteRule(), (_) =>
        {
            if (_communicator.IsEnabled)
            {
                _communicator.Abort();
                TrackerBase.Say(x => x.Muted, wait: true);
                _communicator.Disable();
                TrackerBase.AddUndo(() =>
                {
                    _communicator.Enable();
                    TrackerBase.Say(x => x.ActionUndone);
                });
            }

        });

        AddCommand("Unmute", GetUnmuteRule(), (_) =>
        {
            if (!_communicator.IsEnabled)
            {
                _communicator.Enable();
                TrackerBase.Say(x => x.Unmuted);
                TrackerBase.AddUndo(() =>
                {
                    _communicator.Disable();
                });
            }
        });

        AddCommand("Beat game", GetBeatGameRule(), (_) =>
        {
            TrackerBase.GameStateTracker.GameBeaten(false);
        });
    }
}
