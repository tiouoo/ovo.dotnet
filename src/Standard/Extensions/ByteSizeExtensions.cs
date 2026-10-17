namespace Standard.Extensions;

public static class ByteSizeExtensions
{
    private static readonly string[] Units = { "bytes", "KiB", "MiB", "GiB", "TiB", "PiB", "EiB" };

    public static string ToHumanReadableSize(this double bytes, int decimalPlaces = 2)
    {
        var (value, unit) = bytes.ToReadableSize(decimalPlaces);
        return unit == "bytes" ? $"{Math.Round(value)} {unit}" : $"{value.ToString($"F{decimalPlaces}")} {unit}";
    }

    public static string ToHumanReadableSize(this int bytes, int decimalPlaces = 2)
    {
        return ((double)bytes).ToHumanReadableSize(decimalPlaces);
    }

    public static string ToHumanReadableSize(this long bytes, int decimalPlaces = 2)
    {
        return ((double)bytes).ToHumanReadableSize(decimalPlaces);
    }

    public static (double Value, string Unit) ToReadableSize(this double bytes, int decimalPlaces = 2)
    {
        if (!double.IsFinite(bytes) || bytes < 0) throw new ArgumentException("字节数不能为负数");

        ArgumentOutOfRangeException.ThrowIfNegative(decimalPlaces);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(decimalPlaces, 15);
        var value = bytes;
        var unitIndex = 0;

        while (unitIndex < Units.Length - 1 && value >= 1024)
        {
            value /= 1024.0;
            unitIndex++;
        }

        return (Math.Round(value, decimalPlaces), Units[unitIndex]);
    }

    public static (double Value, string Unit) ToReadableSize(this int bytes, int decimalPlaces = 2)
    {
        return ((double)bytes).ToReadableSize(decimalPlaces);
    }
}