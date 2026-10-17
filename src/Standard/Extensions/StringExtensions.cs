using System.Globalization;
using System.Text;

namespace Standard.Extensions;

public static class StringExtensions
{
    public static bool IsNullOrWhiteSpace(this string? str)
    {
        return string.IsNullOrWhiteSpace(str);
    }

    public static string Truncate(this string? str, int maxLength, string suffix = "...")
    {
        ArgumentOutOfRangeException.ThrowIfNegative(maxLength);
        if (string.IsNullOrEmpty(str) || str.Length <= maxLength) return str ?? string.Empty;
        return str.Substring(0, maxLength) + suffix;
    }

    public static int ToInt(this string? str, int defaultValue = 0, IFormatProvider? provider = null)
    {
        return int.TryParse(str, NumberStyles.Integer, provider, out var result) ? result : defaultValue;
    }

    public static string ToBase64(this string text, Encoding? encoding = null)
    {
        ArgumentNullException.ThrowIfNull(text);
        return Convert.ToBase64String((encoding ?? Encoding.UTF8).GetBytes(text));
    }

    public static string FromBase64(this string text, Encoding? encoding = null)
    {
        ArgumentNullException.ThrowIfNull(text);
        return (encoding ?? Encoding.UTF8).GetString(Convert.FromBase64String(text));
    }
}
