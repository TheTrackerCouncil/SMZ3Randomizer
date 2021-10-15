using System;
using System.Text.Json;
using System.Text.Json.Serialization;

using Randomizer.SMZ3.Tracking.Vocabulary;

namespace Randomizer.SMZ3.Tracking.Converters
{
    internal class SchrodingersStringItemConverter : JsonConverter<SchrodingersString.Item>
    {
        public override SchrodingersString.Item? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var text = reader.GetString();
                return new SchrodingersString.Item(text!);
            }
            else if (reader.TokenType == JsonTokenType.StartArray)
            {
                reader.Read();
                var text = reader.GetString();
                if (text == null)
                    throw new JsonException("Expected a string at the start of the array.");

                reader.Read();
                var weight = reader.GetDouble();

                reader.Read();
                if (reader.TokenType != JsonTokenType.EndArray)
                    throw new JsonException("Expected end of array.");

                return new SchrodingersString.Item(text, weight);
            }

            throw new JsonException($"Unexpected {reader.TokenType} at {reader.TokenStartIndex} when parsing phrase.");
        }

        public override void Write(Utf8JsonWriter writer, SchrodingersString.Item value, JsonSerializerOptions options)
        {
            if (value.Weight == SchrodingersString.Item.DefaultWeight)
                writer.WriteStringValue(value.Text);
            else
            {
                writer.WriteStartArray();
                writer.WriteStringValue(value.Text);
                writer.WriteNumberValue(value.Weight);
                writer.WriteEndArray();
            }
        }
    }
}
