using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Recognition;
using System.Text;

namespace Randomizer.SMZ3.Tracking
{
    public class GrammarBuilder
    {
        private System.Speech.Recognition.GrammarBuilder _grammar;
        private List<string> _elements;

        public GrammarBuilder()
        {
            _grammar = new();
            _elements = new();
        }

        public GrammarBuilder(IEnumerable<GrammarBuilder> choices)
            : this()
        {
            _grammar.Append(new Choices(choices.Select(x => (System.Speech.Recognition.GrammarBuilder)x).ToArray()));
            foreach (var choice in choices)
                _elements.Add(choice.ToString() + "\n");
        }

        public static implicit operator System.Speech.Recognition.GrammarBuilder(GrammarBuilder self) => self._grammar;

        public static GrammarBuilder Combine(params GrammarBuilder[] choices)
        {
            return new GrammarBuilder(choices);
        }

        public GrammarBuilder Append(string phrase)
        {
            _grammar.Append(phrase);
            _elements.Add(phrase);
            return this;
        }

        public GrammarBuilder Append(string key, Choices choices)
        {
            _grammar.Append(new SemanticResultKey(key, choices));
            _elements.Add($"<{key}>");
            return this;
        }

        public GrammarBuilder OneOf(params string[] choices)
        {
            _grammar.Append(new Choices(choices));
            _elements.Add($"[{string.Join(',', choices)}]");
            return this;
        }

        public Grammar Build(string name)
        {
            return new Grammar(this)
            {
                Name = name
            };
        }

        public override string ToString()
            => string.Join(' ', _elements);
    }
}
