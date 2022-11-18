using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Randomizer.Data.Configuration.ConfigTypes;

namespace Randomizer.Data.Configuration.Converters
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
                var items = JsonSerializer.Deserialize<IEnumerable<SchrodingersString.Possibility>>(ref reader, options);
                if (items == null)
                    throw new JsonException();
                return new SchrodingersString(items);
            }

            throw new JsonException("Unsupported token type for SchrodingersString.");
        }

        public override void Write(Utf8JsonWriter writer, SchrodingersString value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, (IEnumerable<SchrodingersString.Possibility>)value, options);
        }
    }
}
