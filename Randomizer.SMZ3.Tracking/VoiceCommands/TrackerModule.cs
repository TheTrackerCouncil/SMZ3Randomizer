using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Speech.Recognition;

namespace Randomizer.SMZ3.Tracking.VoiceCommands
{
    public abstract class TrackerModule
    {
        protected const string DungeonKey = "DungeonName";
        protected const string ItemNameKey = "ItemName";

        private Dictionary<string, IEnumerable<string>> _syntax = new();

        protected TrackerModule(Tracker tracker)
        {
            Tracker = tracker;
        }

        public IReadOnlyDictionary<string, IEnumerable<string>> Syntax
            => _syntax.ToImmutableDictionary();

        protected Tracker Tracker { get; }

        protected IList<Grammar> Grammars { get; }
            = new List<Grammar>();

        public void LoadInto(SpeechRecognitionEngine engine)
        {
            foreach (var grammar in Grammars)
                engine.LoadGrammar(grammar);
        }

        protected static ZeldaDungeon GetDungeonFromResult(Tracker tracker, RecognitionResult result)
        {
            var dungeonName = (string)result.Semantics[DungeonKey].Value;
            var dungeon = tracker.Dungeons.SingleOrDefault(x => x.Name.Contains(dungeonName, StringComparison.OrdinalIgnoreCase)
                                                             || x.Abbreviation.Equals(dungeonName, StringComparison.OrdinalIgnoreCase));
            return dungeon ?? throw new Exception($"Could not find recognized dungeon '{dungeonName}'.");
        }

        protected static ItemData GetItemFromResult(Tracker tracker, RecognitionResult result, out string itemName)
        {
            itemName = (string)result.Semantics[ItemNameKey].Value;
            var itemData = tracker.FindItemByName(itemName);

            return itemData ?? throw new Exception($"Could not find recognized item '{itemName}' (\"{result.Text}\")");
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

        protected Choices GetItemNames()
        {
            var itemNames = new Choices();
            foreach (var itemData in Tracker.Items)
            {
                foreach (var name in itemData.Name)
                    itemNames.Add(new SemanticResultValue(name.ToString(), name.ToString()));

                if (itemData.Stages != null)
                {
                    foreach (var stageName in itemData.Stages.SelectMany(x => x.Value))
                    {
                        itemNames.Add(new SemanticResultValue(stageName.ToString(), stageName.ToString()));
                    }
                }
            }

            return itemNames;
        }

        protected Choices GetDungeonNames()
        {
            var dungeonNames = new Choices();
            foreach (var dungeon in Tracker.Dungeons)
            {
                foreach (var name in dungeon.Name)
                    dungeonNames.Add(new SemanticResultValue(name.Text, name.Text));
                dungeonNames.Add(new SemanticResultValue(dungeon.Abbreviation, dungeon.Abbreviation));
            }

            return dungeonNames;
        }
    }
}
