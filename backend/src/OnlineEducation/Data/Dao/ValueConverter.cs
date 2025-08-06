
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using OnlineEducation.Utils;
using System.Text.Json;

namespace OnlineEducation.Data.Dao;

public class ElementMetadataConverter : ValueConverter<ElementMetadata?, string>
{
    public ElementMetadataConverter()
        : base(
            v => JsonSerializer.Serialize(v, CreateSerializerOptions()),
            v => JsonSerializer.Deserialize<ElementMetadata>(v, CreateSerializerOptions())
        )
    {
    }

    private static JsonSerializerOptions CreateSerializerOptions()
    {
        var options = new JsonSerializerOptions
        {
            Converters =
            {
                new DictionaryIntConverter(),
                new DictionaryStringsConverter()
            }
        };
        return options;
    }
}