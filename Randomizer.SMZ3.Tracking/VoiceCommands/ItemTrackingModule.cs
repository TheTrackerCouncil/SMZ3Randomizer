using System;
using System.Linq;
using System.Speech.Recognition;

namespace Randomizer.SMZ3.Tracking.VoiceCommands
{
    public class ItemTrackingModule : TrackerModule
    {
        private const string ItemNameKey = "ItemName";

        public ItemTrackingModule(Tracker tracker)
            : base(tracker)
        {
            AddCommand("TrackItemRule", GetTrackItemRule(), (tracker, result) =>
            {
                var itemName = (string)result.Semantics[ItemNameKey].Value;
                var itemData = Tracker.FindItemByName(itemName);
                if (itemData == null)
                    throw new Exception($"Could not find item '{itemName}' (\"{result.Text}\")");

                tracker.TrackItem(itemData, itemName, result.Confidence);
            });
        }

        private GrammarBuilder GetTrackItemRule()
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

            return new GrammarBuilder()
                .Append("Hey tracker,")
                .OneOf("track", "please track")
                .Append(ItemNameKey, itemNames);
        }
    }
}
