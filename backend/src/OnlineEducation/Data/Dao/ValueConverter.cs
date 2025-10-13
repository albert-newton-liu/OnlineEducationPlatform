using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using OnlineEducation.Utils;
using System.Text.Json;

namespace OnlineEducation.Data.Dao;

/// <summary>
/// Converts <see cref="ElementMetadata"/> objects to and from their JSON string representation
/// for storage in the database using Entity Framework Core.
/// </summary>
public class ElementMetadataConverter : ValueConverter<ElementMetadata?, string>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ElementMetadataConverter"/> class.
    /// Sets up serialization and deserialization logic for <see cref="ElementMetadata"/>.
    /// </summary>
    public ElementMetadataConverter()
        : base(
            v => JsonSerializer.Serialize(v, CreateSerializerOptions()),
            v => JsonSerializer.Deserialize<ElementMetadata>(v, CreateSerializerOptions())
        )
    {
    }

    /// <summary>
    /// Creates custom <see cref="JsonSerializerOptions"/> with required converters for serialization.
    /// </summary>
    /// <returns>The configured <see cref="JsonSerializerOptions"/>.</returns>
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