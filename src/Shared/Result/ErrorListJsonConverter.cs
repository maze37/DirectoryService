using System.Text.Json;
using System.Text.Json.Serialization;

namespace Shared.Result;

public class ErrorListJsonConverter : JsonConverter<ErrorList>
{
    public override ErrorList? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var errors = JsonSerializer.Deserialize<List<Error>>(ref reader, options);
        return errors == null ? null : new ErrorList(errors);
    }

    public override void Write(Utf8JsonWriter writer, ErrorList value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value.ToList(), options);
    }
}