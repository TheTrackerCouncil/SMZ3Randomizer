using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Randomizer.SMZ3.Tracking.Vocabulary
{
    /// <summary>
    /// Represents a phrase to be spoken.
    /// </summary>
    [JsonConverter(typeof(PhraseConverter))]
    [DebuggerDisplay("{Text} Weight = {Weight}")]
    public class Phrase
    {
        /// <summary>
        /// The weight of phrases that do not have an explicit weight assigned
        /// to them.
        /// </summary>
        public const double DefaultWeight = 1.0d;

        /// <summary>
        /// Initializes a new instance of the <see cref="Phrase"/> class with
        /// the specified phrase and the default weight of 1.
        /// </summary>
        /// <param name="text">The text to be spoken.</param>
        public Phrase(string text) : this(text, DefaultWeight)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Phrase"/> class with
        /// the specified phrase and weight.
        /// </summary>
        /// <param name="text">The text to be spoken.</param>
        /// <param name="weight">
        /// The weight for the item, based on a default of 1.
        /// </param>
        public Phrase(string text, double weight)
        {
            if (weight < 0)
                throw new ArgumentOutOfRangeException(nameof(weight), weight, "Weight cannot be less than zero.");

            Text = text;
            Weight = weight;
        }

        /// <summary>
        /// Gets a string containing the phrase.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Gets the weight associated with the phrase.
        /// </summary>
        [DefaultValue(DefaultWeight)]
        public double Weight { get; }

        public static implicit operator string(Phrase phrase) => phrase.Text;

        public static implicit operator Phrase(string text) => new(text);

        public override int GetHashCode() => Text.GetHashCode();

        public override string ToString() => Text;
    }
}
