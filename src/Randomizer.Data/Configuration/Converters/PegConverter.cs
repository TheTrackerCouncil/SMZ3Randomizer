using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Randomizer.Data.Configuration.Converters
{
    internal class PegConverter : JsonConverter<Peg>
    {
        public override Peg? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartArray)
                throw new JsonException("Expected start of array.");

            reader.Read();
            if (reader.TokenType != JsonTokenType.Number)
                throw new JsonException("Expected number.");

            var column = reader.GetInt32();

            reader.Read();
            if (reader.TokenType != JsonTokenType.Number)
                throw new JsonException("Expected number.");

            var row = reader.GetInt32();

            reader.Read();
            if (reader.TokenType != JsonTokenType.EndArray)
                throw new JsonException("Expected end of array.");

            return new Peg(column, row);
        }

        public override void Write(Utf8JsonWriter writer, Peg value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            writer.WriteNumberValue(value.Column);
            writer.WriteNumberValue(value.Row);
            writer.WriteEndArray();
        }
    }
}
