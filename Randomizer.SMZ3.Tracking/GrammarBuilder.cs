using System;
using System.Linq;
using System.Speech.Recognition;

namespace Randomizer.SMZ3.Tracking
{
    public class GrammarBuilder
    {
        private System.Speech.Recognition.GrammarBuilder _builder;

        public GrammarBuilder()
        {
            _builder = new();
        }

        public static implicit operator System.Speech.Recognition.GrammarBuilder(GrammarBuilder self) => self._builder;

        public static GrammarBuilder Combine(params GrammarBuilder[] choices)
        {
            var builder = new GrammarBuilder();
            builder.Append(new Choices(choices.Select(x => (System.Speech.Recognition.GrammarBuilder)x).ToArray()));
            return builder;
        }

        public GrammarBuilder Append(string phrase)
        {
            _builder.Append(phrase);
            return this;
        }

        public GrammarBuilder Append(string key, Choices choices)
        {
            _builder.Append(new SemanticResultKey(key, choices));
            return this;
        }

        public GrammarBuilder Append(Choices choices)
        {
            _builder.Append(choices);
            return this;
        }

        public Grammar Build(string name)
        {
            return new Grammar(this)
            {
                Name = name
            };
        }
    }
}
