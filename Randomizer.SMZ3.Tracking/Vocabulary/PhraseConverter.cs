using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Randomizer.SMZ3.Tracking.Vocabulary
{
    internal class PhraseConverter : JsonConverter<Phrase>
    {
        public override Phrase? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var text = reader.GetString();
                return new Phrase(text!);
            }

            throw new JsonException();
        }

        public override void Write(Utf8JsonWriter writer, Phrase value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.Text);
        }
    }
}
