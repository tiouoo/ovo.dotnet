namespace Standard.Extensions;

public static class ObjectExtensions
{
    public static bool IsNull(this object? obj)
    {
        return obj == null;
    }

    public static string ToBase64(this byte[] bytes)
    {
        var base64String = Convert.ToBase64String(bytes);
        return base64String;
    }
    public static T ThrowIfNull<T>(this T? obj, string paramName, string? message = null) where T : class
    {
        if (obj == null)
            throw new ArgumentNullException(paramName, message ?? $"{paramName} cannot be null.");
        return obj;
    }
}
