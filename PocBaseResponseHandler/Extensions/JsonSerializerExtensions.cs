namespace PocBaseResponseHandler.Extensions;

using System.Text.Json;
using System.Text.Json.Serialization;

public static class JsonSerializerExtensions
{
    public static string ToIndentedIgnoreNullJson(this object? self)
    {
        if (self is null)
        {
            return string.Empty;
        }

        var options = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = true
        };

        return JsonSerializer.Serialize(self, options);
    }

    public static bool TryDeserialize<T>(this string self, out T? result)
    {
        result = default;

        try
        {
            if (string.IsNullOrWhiteSpace(self))
            {
                return false;
            }

            var options = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            result = JsonSerializer.Deserialize<T>(self, options);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
