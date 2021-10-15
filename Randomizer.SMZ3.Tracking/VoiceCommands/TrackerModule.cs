using System;
using System.Collections.Generic;
using System.Speech.Recognition;

namespace Randomizer.SMZ3.Tracking.VoiceCommands
{
    public abstract class TrackerModule
    {
        protected TrackerModule(Tracker tracker)
        {
            Tracker = tracker;
        }

        protected Tracker Tracker { get; }

        protected IList<Grammar> Grammars { get; }
            = new List<Grammar>();

        public void LoadInto(SpeechRecognitionEngine engine)
        {
            foreach (var grammar in Grammars)
                engine.LoadGrammar(grammar);
        }

        protected void AddCommand(string ruleName, string phrase,
            Action<Tracker, RecognitionResult> executeCommand)
        {
            var builder = new GrammarBuilder()
                .Append(phrase);

            AddCommand(ruleName, builder, executeCommand);
        }

        protected void AddCommand(string ruleName, string[] phrases,
            Action<Tracker, RecognitionResult> executeCommand)
        {
            var builder = new GrammarBuilder()
                .Append(new Choices(phrases));

            AddCommand(ruleName, builder, executeCommand);
        }

        protected void AddCommand(string ruleName, GrammarBuilder grammarBuilder,
            Action<Tracker, RecognitionResult> executeCommand)
        {
            var grammar = grammarBuilder.Build(ruleName);
            grammar.SpeechRecognized += (sender, e) => executeCommand(Tracker, e.Result);
            Grammars.Add(grammar);
        }
    }
}
