using System;
using System.Text.Json.Serialization;

namespace Randomizer.SMZ3.Tracking.Vocabulary
{
    /// <summary>
    /// Represents a phrase to be spoken.
    /// </summary>
    [JsonConverter(typeof(PhraseConverter))]
    public class Phrase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Phrase"/> class.
        /// </summary>
        /// <param name="text">The text to be spoken.</param>
        public Phrase(string text)
        {
            Text = text;
        }

        /// <summary>
        /// Gets a string containing the phrase.
        /// </summary>
        public string Text { get; }

        public static implicit operator string(Phrase phrase) => phrase.Text;

        public static implicit operator Phrase(string text) => new(text);

        public override int GetHashCode() => Text.GetHashCode();
    }
}
