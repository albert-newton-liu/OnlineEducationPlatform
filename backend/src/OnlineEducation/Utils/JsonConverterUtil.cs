using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using OnlineEducation.Data.Dao;

namespace OnlineEducation.Utils;

/// <summary>
/// Custom JSON converter for Dictionary<string, int>.
/// </summary>  
public class DictionaryIntConverter : JsonConverter<Dictionary<string, int>>
{
    /// <summary>
    /// Reads and converts the JSON to a Dictionary<string, int>.
    /// </summary>
    public override Dictionary<string, int>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject) throw new JsonException();
        var dictionary = new Dictionary<string, int>();
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject) return dictionary;
            if (reader.TokenType != JsonTokenType.PropertyName) throw new JsonException();
            string? key = reader.GetString();
            if (string.IsNullOrEmpty(key)) continue;
            reader.Read();
            if (reader.TokenType != JsonTokenType.Number) throw new JsonException("Dictionary value must be a number.");
            if (reader.TryGetInt32(out int value)) dictionary.Add(key, value);
            else throw new JsonException("Could not read byte value.");
        }
        return dictionary;
    }

    /// <summary>
    /// Writes the Dictionary<string, int> as JSON.
    /// </summary>
    public override void Write(Utf8JsonWriter writer, Dictionary<string, int> value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        foreach (var kvp in value) writer.WriteNumber(kvp.Key, kvp.Value);
        writer.WriteEndObject();
    }
}

/// <summary>
/// Custom JSON converter for Dictionary<string, string>.
/// </summary>
public class DictionaryStringsConverter : JsonConverter<Dictionary<string, string>>
{
    /// <summary>
    /// Reads and converts the JSON to a Dictionary<string, string>.
    /// </summary>
    public override Dictionary<string, string>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject) throw new JsonException();
        var dictionary = new Dictionary<string, string>();
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject) return dictionary;
            if (reader.TokenType != JsonTokenType.PropertyName) throw new JsonException();
            string? key = reader.GetString();
            if (string.IsNullOrEmpty(key)) continue;
            reader.Read();
            if (reader.TokenType != JsonTokenType.String) throw new JsonException("Dictionary value must be a string.");
            string? value = reader.GetString();
            dictionary.Add(key, value ?? "");
        }
        return dictionary;
    }

    /// <summary>
    /// Writes the Dictionary<string, string> as JSON.
    /// </summary>
    public override void Write(Utf8JsonWriter writer, Dictionary<string, string> value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        foreach (var kvp in value) writer.WriteString(kvp.Key, kvp.Value);
        writer.WriteEndObject();
    }
}

