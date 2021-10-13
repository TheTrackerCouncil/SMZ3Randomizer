using System;
using System.Linq;
using System.Speech.Recognition;

namespace Randomizer.SMZ3.Tracking.VoiceCommands
{
    public class TrackItemsCommand : IVoiceCommand
    {
        private const string ItemNameKey = "ItemName";

        public TrackItemsCommand(Tracker tracker)
        {
            Tracker = tracker;
        }

        public event EventHandler<ItemTrackedEventArgs>? ItemTracked;

        public Tracker Tracker { get; }

        public Grammar BuildGrammar()
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

            var trackItemPhrase = new GrammarBuilder()
                .Append("Hey tracker, track")
                .Append(ItemNameKey, itemNames);

            var trackItemPleasePhrase = new GrammarBuilder()
                .Append("Hey tracker, please track")
                .Append(ItemNameKey, itemNames);

            var trackItemCommands = GrammarBuilder.Combine(
                trackItemPhrase,
                trackItemPleasePhrase);

            var grammar = trackItemCommands.Build(nameof(TrackItemsCommand));
            grammar.SpeechRecognized += Grammar_SpeechRecognized;
            return grammar;
        }

        protected virtual void OnItemTracked(ItemTrackedEventArgs e)
        {
            ItemTracked?.Invoke(this, e);
        }

        private void Grammar_SpeechRecognized(object? sender, SpeechRecognizedEventArgs e)
        {
            var itemName = (string)e.Result.Semantics[ItemNameKey].Value;
            var itemData = Tracker.FindItemByName(itemName);
            if (itemData == null)
                throw new Exception($"Could not find item '{itemName}' (\"{e.Result.Text}\")");

            OnItemTracked(new ItemTrackedEventArgs(itemData, itemName, e.Result.Confidence));
        }
    }
}
