using System.Collections.Generic;
using System.Linq;
using System.Speech.Recognition;

namespace Randomizer.SMZ3.Tracking.VoiceCommands
{
    /// <summary>
    /// Constructs a speech recognition grammar.
    /// </summary>
    public class GrammarBuilder
    {
        private readonly System.Speech.Recognition.GrammarBuilder _grammar;
        private readonly List<string> _elements;

        /// <summary>
        /// Initializes a new empty instance of the <see cref="GrammarBuilder"/>
        /// class.
        /// </summary>
        public GrammarBuilder()
        {
            _grammar = new();
            _elements = new();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GrammarBuilder"/> class
        /// that combines the specified grammars into a single choice.
        /// </summary>
        /// <param name="choices">The grammars to choose from.</param>
        public GrammarBuilder(IEnumerable<GrammarBuilder> choices)
            : this()
        {
            _grammar.Append(new Choices(choices.Select(x => (System.Speech.Recognition.GrammarBuilder)x).ToArray()));
            foreach (var choice in choices)
                _elements.Add(choice + "\n");
        }

        /// <summary>
        /// Converts the grammar builder to the System.Speech grammar builder.
        /// </summary>
        /// <param name="self">The grammar builder to convert.</param>
        public static implicit operator System.Speech.Recognition.GrammarBuilder(GrammarBuilder self) => self._grammar;

        /// <summary>
        /// Combines the specified grammars into a single grammar.
        /// </summary>
        /// <param name="choices">
        /// The possible grammars to choose from in the new grammar.
        /// </param>
        /// <returns>
        /// A new <see cref="GrammarBuilder"/> that represents a choice of one
        /// of <paramref name="choices"/>.
        /// </returns>
        public static GrammarBuilder Combine(params GrammarBuilder[] choices)
        {
            return new GrammarBuilder(choices);
        }

        /// <summary>
        /// Adds the specified text to the end of the grammar.
        /// </summary>
        /// <param name="text">The text to recognize.</param>
        /// <returns>This instance.</returns>
        public GrammarBuilder Append(string text)
        {
            _grammar.Append(text);
            _elements.Add(text);
            return this;
        }

        /// <summary>
        /// Adds a choice that can be retrieved later using the specified
        /// semantic result key.
        /// </summary>
        /// <param name="key">
        /// The key used to retrieve the recognized choice.
        /// </param>
        /// <param name="choices">
        /// The choices to represent in the grammar.
        /// </param>
        /// <returns>This instance.</returns>
        public GrammarBuilder Append(string key, Choices choices)
        {
            _grammar.Append(new SemanticResultKey(key, choices));
            _elements.Add($"<{key}>");
            return this;
        }

        /// <summary>
        /// Adds a choice.
        /// </summary>
        /// <param name="choices">
        /// The choices to represent in the grammar.
        /// </param>
        /// <returns>This instance.</returns>
        public GrammarBuilder OneOf(params string[] choices)
        {
            _grammar.Append(new Choices(choices));
            _elements.Add($"[{string.Join('/', choices)}]");
            return this;
        }

        /// <summary>
        /// Adds the specified optional text to the end of the grammar.
        /// </summary>
        /// <param name="text">
        /// The text that may or may not be recognized.
        /// </param>
        /// <returns>This instance.</returns>
        public GrammarBuilder Optional(string text)
        {
            _grammar.Append(text, 0, 1);
            _elements.Add($"({text})");
            return this;
        }

        /// <summary>
        /// Adds an optional choice.
        /// </summary>
        /// <param name="choices">The choices to represent in the grammar.</param>
        /// <returns>This instance.</returns>
        public GrammarBuilder Optional(params string[] choices)
        {
            _grammar.Append(new Choices(choices), 0, 1);
            _elements.Add($"({string.Join('/', choices)})");
            return this;
        }

        /// <summary>
        /// Builds the grammar.
        /// </summary>
        /// <param name="name">The name of the grammar rule.</param>
        /// <returns>A new <see cref="Grammar"/>.</returns>
        public Grammar Build(string name)
        {
            return new Grammar(this)
            {
                Name = name
            };
        }

        /// <summary>
        /// Returns a string representing the grammar syntax.
        /// </summary>
        /// <returns>A string representing the grammar syntax.</returns>
        public override string ToString()
            => string.Join(' ', _elements);
    }
}
