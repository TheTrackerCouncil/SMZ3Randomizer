using System;
using System.Speech.Recognition;

using Microsoft.Extensions.Logging;

using Randomizer.SMZ3.Tracking.Services;

namespace Randomizer.SMZ3.Tracking.VoiceCommands
{
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
        /// <param name="logger">Used to write logging information.</param>
        public MetaModule(Tracker tracker, IItemService itemService, ILogger<MetaModule> logger, ICommunicator communicator)
            : base(tracker, itemService, logger)
        {
            _communicator = communicator;

            AddCommand("Repeat that", GetRepeatThatRule(), (tracker, result) =>
            {
                tracker.Repeat();
            });

            AddCommand("Shut up", GetShutUpRule(), (tracker, result) =>
            {
                tracker.ShutUp();
            });

            AddCommand("Temporarily change threshold setting", GetIncreaseThresholdGrammar(), (tracker, result) =>
            {
                var modifier = (int)result.Semantics[ModifierKey].Value;
                var thresholdSetting = (int)result.Semantics[ThresholdSettingKey].Value;
                var value = (float)(double)result.Semantics[ValueKey].Value;

                var adjustment = modifier * value;
                switch (thresholdSetting)
                {
                    case ThresholdSetting_Recognition:
                        Tracker.Options.MinimumRecognitionConfidence += adjustment;
                        Tracker.Say(Tracker.Responses.TrackerSettingChanged.Format(
                            "recognition threshold", $"{Tracker.Options.MinimumRecognitionConfidence:P0}"));
                        logger.LogInformation("Temporarily changed recognition threshold to {newValue}", Tracker.Options.MinimumRecognitionConfidence);
                        break;

                    case ThresholdSetting_Execution:
                        Tracker.Options.MinimumExecutionConfidence += adjustment;
                        Tracker.Say(Tracker.Responses.TrackerSettingChanged.Format(
                            "execution threshold", $"{Tracker.Options.MinimumExecutionConfidence:P0}"));
                        logger.LogInformation("Temporarily changed execution threshold to {newValue}", Tracker.Options.MinimumExecutionConfidence);
                        break;

                    case ThresholdSetting_Sass:
                        Tracker.Options.MinimumSassConfidence += adjustment;
                        Tracker.Say(Tracker.Responses.TrackerSettingChanged.Format(
                            "sass threshold", $"{Tracker.Options.MinimumSassConfidence:P0}"));
                        logger.LogInformation("Temporarily changed sass threshold to {newValue}", Tracker.Options.MinimumSassConfidence);
                        break;

                    default:
                        throw new ArgumentException($"The threshold setting '{thresholdSetting}' was not recognized.");
                }
            });

            AddCommand("Pause timer", GetPauseTimerRule(), (tracker, result) =>
            {
                tracker.PauseTimer();
            });

            AddCommand("Start timer", GetResumeTimerRule(), (tracker, result) =>
            {
                tracker.StartTimer();
            });

            AddCommand("Reset timer", GetResetTimerRule(), (tracker, result) =>
            {
                tracker.ResetTimer();
            });

            AddCommand("Mute", GetMuteRule(), (tracker, result) =>
            {
                tracker.Say(x => x.Muted);
                _communicator.Disable();
            });

            AddCommand("Unmute", GetUnmuteRule(), (tracker, result) =>
            {
                _communicator.Enable();
                tracker.Say(x => x.Unmuted);
            });
        }

        private static Choices GetIncreaseDecrease()
        {
            var modifiers = new Choices();
            modifiers.Add(new SemanticResultValue("increase", 1));
            modifiers.Add(new SemanticResultValue("decrease", -1));
            return modifiers;
        }

        private static Choices GetOneThroughTenPercent()
        {
            var values = new Choices();
            values.Add(new SemanticResultValue("one percent", 0.01));
            values.Add(new SemanticResultValue("two percent", 0.02));
            values.Add(new SemanticResultValue("three percent", 0.03));
            values.Add(new SemanticResultValue("four percent", 0.04));
            values.Add(new SemanticResultValue("five percent", 0.05));
            values.Add(new SemanticResultValue("six percent", 0.06));
            values.Add(new SemanticResultValue("seven percent", 0.07));
            values.Add(new SemanticResultValue("eight percent", 0.08));
            values.Add(new SemanticResultValue("nine percent", 0.09));
            values.Add(new SemanticResultValue("ten percent", 0.10));
            return values;
        }

        private static Choices GetThresholdSettings()
        {
            var settings = new Choices();
            settings.Add(new SemanticResultValue("recognition", ThresholdSetting_Recognition));
            settings.Add(new SemanticResultValue("execution", ThresholdSetting_Execution));
            settings.Add(new SemanticResultValue("sass", ThresholdSetting_Sass));
            return settings;
        }

        private GrammarBuilder GetRepeatThatRule()
        {
            return new GrammarBuilder()
                .Append("Hey tracker, ")
                .OneOf("can you repeat that?",
                    "can you please repeat that?",
                    "what was that?",
                    "repeat that",
                    "please repeat that");
        }

        private GrammarBuilder GetShutUpRule()
        {
            return new GrammarBuilder()
                .Append("Hey tracker, ")
                .Optional("please")
                .OneOf("shut up", "be quiet", "stop talking");
        }

        private GrammarBuilder GetIncreaseThresholdGrammar()
        {
            var modifiers = GetIncreaseDecrease();
            var settings = GetThresholdSettings();
            var values = GetOneThroughTenPercent();
            return new GrammarBuilder()
                .Append("Hey tracker,")
                .Append(ModifierKey, modifiers)
                .Optional("minimum")
                .Append(ThresholdSettingKey, settings)
                .OneOf("threshold", "confidence")
                .Append("by")
                .Append(ValueKey, values);
        }

        private GrammarBuilder GetPauseTimerRule()
        {
            return new GrammarBuilder()
                .Append("Hey tracker, ")
                .Optional("please")
                .OneOf("pause the timer", "stop the timer");
        }

        private GrammarBuilder GetResumeTimerRule()
        {
            return new GrammarBuilder()
                .Append("Hey tracker, ")
                .Optional("please")
                .OneOf("resume the timer", "start the timer");
        }

        private GrammarBuilder GetResetTimerRule()
        {
            return new GrammarBuilder()
                .Append("Hey tracker, ")
                .Optional("please")
                .OneOf("reset the timer");
        }

        private GrammarBuilder GetMuteRule()
        {
            return new GrammarBuilder()
                .Append("Hey tracker, ")
                .Optional("please")
                .OneOf("mute yourself", "silence yourself");
        }

        private GrammarBuilder GetUnmuteRule()
        {
            return new GrammarBuilder()
                .Append("Hey tracker, ")
                .Optional("please")
                .OneOf("unmute yourself", "unsilence yourself");
        }
    }
}
