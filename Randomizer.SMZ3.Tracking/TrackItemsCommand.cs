using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Recognition;
using System.Text;
using System.Threading.Tasks;

namespace Randomizer.SMZ3.Tracking
{
    public class TrackItemsCommand : IVoiceCommand
    {
        private const string ItemNameKey = "ItemName";

        public event EventHandler<ItemTrackedEventArgs> ItemTracked;

        public Grammar BuildGrammar()
        {
            var itemChoices = new Choices();
            foreach (var itemType in Enum.GetValues<ItemType>())
            {
                var name = itemType.GetDescription();
                itemChoices.Add(new SemanticResultValue(name, (int)itemType));
            }

            var trackItemPhrase = new GrammarBuilder()
                .Append("Hey tracker, track")
                .Append(ItemNameKey, itemChoices);

            var trackItemPleasePhrase = new GrammarBuilder()
                .Append("Hey tracker, please track")
                .Append(ItemNameKey, itemChoices);

            var trackItemCommands = GrammarBuilder.Combine(
                trackItemPhrase,
                trackItemPleasePhrase);

            var grammar = trackItemCommands.Build(nameof(TrackItemsCommand));
            grammar.SpeechRecognized += Grammar_SpeechRecognized;
            return grammar;
        }

        private void Grammar_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            var value = (int)e.Result.Semantics[ItemNameKey].Value;
            var itemType = (ItemType)value;
            OnItemTracked(new ItemTrackedEventArgs(itemType, e.Result.Confidence));
        }

        protected virtual void OnItemTracked(ItemTrackedEventArgs e)
        {
            ItemTracked?.Invoke(this, e);
        }
    }
}
