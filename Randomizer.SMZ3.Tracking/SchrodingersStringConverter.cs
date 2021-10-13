using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using Randomizer.SMZ3.Tracking.Vocabulary;

namespace Randomizer.SMZ3.Tracking
{
    internal class SchrodingersStringConverter : JsonConverter<SchrodingersString>
    {
        public override SchrodingersString? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var text = reader.GetString();
                return new SchrodingersString(text!);
            }

            if (reader.TokenType == JsonTokenType.StartArray)
            {
                var items = JsonSerializer.Deserialize<IEnumerable<SchrodingersString.Item>>(ref reader, options);
                if (items == null)
                    throw new JsonException();
                return new SchrodingersString(items);
            }

            throw new JsonException("Unsupported token type for SchrodingersString.");
        }

        public override void Write(Utf8JsonWriter writer, SchrodingersString value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, (IEnumerable<SchrodingersString.Item>)value, options);
        }
    }
}
