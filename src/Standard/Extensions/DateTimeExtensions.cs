using System.Globalization;

namespace Standard.Extensions;

public static class DateTimeExtensions
{
    public static long ToUnixTimestamp(this DateTime dateTime) => new DateTimeOffset(dateTime).ToUnixTimeSeconds();
    public static long ToUnixTimestamp(this DateTimeOffset dateTime) => dateTime.ToUnixTimeSeconds();
    public static long ToUnixMilliseconds(this DateTime dateTime) => new DateTimeOffset(dateTime).ToUnixTimeMilliseconds();
    public static long ToUnixMilliseconds(this DateTimeOffset dateTime) => dateTime.ToUnixTimeMilliseconds();

    public static string ToRelativeTime(this DateTime dateTime, string language = "zh", DateTime? referenceTime = null,
        bool useRelativeDates = false)
    {
        return new DateTimeOffset(dateTime).ToRelativeTime(language,
            referenceTime.HasValue ? new DateTimeOffset(referenceTime.Value) : null, useRelativeDates);
    }

    public static string ToRelativeTime(this DateTimeOffset dateTime, string language = "zh",
        DateTimeOffset? referenceTime = null, bool useRelativeDates = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(language);
        var code = CultureInfo.GetCultureInfo(language).TwoLetterISOLanguageName;
        if (code is not ("zh" or "en")) throw new NotSupportedException($"Unsupported language: {language}");
        var difference = dateTime - (referenceTime ?? DateTimeOffset.Now);
        var future = difference > TimeSpan.Zero;
        var seconds = Math.Abs(difference.TotalSeconds);
        var english = code == "en";
        if (seconds < 60) return english ? (future ? "in a moment" : "just now") : (future ? "马上" : "刚刚");
        var days = seconds / 86400;
        if (days >= 30 && !useRelativeDates) return dateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        if (days is >= 1 and < 2) return english ? (future ? "tomorrow" : "yesterday") : (future ? "明天" : "昨天");
        var (count, chineseUnit, englishUnit) = seconds switch
        {
            < 3600 => ((long)(seconds / 60), "分钟", "minute"),
            < 86400 => ((long)(seconds / 3600), "小时", "hour"),
            < 2592000 => ((long)days, "天", "day"),
            < 31536000 => ((long)(days / 30), "个月", "month"),
            _ => ((long)(days / 365), "年", "year")
        };
        if (!english) return $"{count} {chineseUnit}{(future ? "后" : "前")}";
        var quantity = $"{count} {englishUnit}{(count == 1 ? "" : "s")}";
        return future ? $"in {quantity}" : $"{quantity} ago";
    }
}
