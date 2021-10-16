using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Speech.Recognition;

namespace Randomizer.SMZ3.Tracking.VoiceCommands
{
    public abstract class TrackerModule
    {
        private Dictionary<string, IEnumerable<string>> _syntax = new();

        protected TrackerModule(Tracker tracker)
        {
            Tracker = tracker;
        }

        protected Tracker Tracker { get; }

        protected IList<Grammar> Grammars { get; }
            = new List<Grammar>();

        public IReadOnlyDictionary<string, IEnumerable<string>> Syntax
            => _syntax.ToImmutableDictionary();

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

            _syntax[ruleName] = new[] { phrase };
            AddCommand(ruleName, builder, executeCommand);
        }

        protected void AddCommand(string ruleName, string[] phrases,
            Action<Tracker, RecognitionResult> executeCommand)
        {
            var builder = new GrammarBuilder()
                .OneOf(phrases);

            _syntax[ruleName] = phrases;
            AddCommand(ruleName, builder, executeCommand);
        }

        protected void AddCommand(string ruleName, GrammarBuilder grammarBuilder,
            Action<Tracker, RecognitionResult> executeCommand)
        {
            _syntax.TryAdd(ruleName, grammarBuilder.ToString().Split('\n', StringSplitOptions.RemoveEmptyEntries));

            var grammar = grammarBuilder.Build(ruleName);
            grammar.SpeechRecognized += (sender, e) => executeCommand(Tracker, e.Result);
            Grammars.Add(grammar);
        }
    }
}
