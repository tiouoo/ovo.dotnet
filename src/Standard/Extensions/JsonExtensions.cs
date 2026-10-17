using System.Text.Encodings.Web;
using System.Text.Json;

namespace Standard.Extensions;

public static class JsonExtensions
{
    public static string AsJson(this object? obj, bool indented = true)
    {
        var options = new JsonSerializerOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
        if (indented)
        {
            options.WriteIndented = true;
            options.IndentSize = 2;
        }

        return JsonSerializer.Serialize(obj, obj?.GetType() ?? typeof(object), options);
    }

    public static T? FromJson<T>(this string json, JsonSerializerOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(json);
        return JsonSerializer.Deserialize<T>(json, options);
    }
}
