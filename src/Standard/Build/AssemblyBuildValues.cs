using System.Reflection;

namespace Standard.Build;

/// <summary>Reads values embedded in a specific assembly, with optional runtime environment fallback.</summary>
public sealed class AssemblyBuildValues
{
    private readonly Dictionary<string, string?> _values;

    public AssemblyBuildValues(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        _values = new Dictionary<string, string?>(StringComparer.Ordinal);
        foreach (var attribute in assembly.GetCustomAttributes<AssemblyMetadataAttribute>())
            _values.TryAdd(attribute.Key, attribute.Value);
    }

    /// <summary>Returns embedded metadata first, then the trimmed environment value, or null.</summary>
    public string? GetValue(string key, string? environmentVariable = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        if (_values.TryGetValue(key, out var embedded) && !string.IsNullOrWhiteSpace(embedded))
            return embedded;

        if (string.IsNullOrWhiteSpace(environmentVariable)) return null;
        var value = Environment.GetEnvironmentVariable(environmentVariable)?.Trim();
        return string.IsNullOrEmpty(value) ? null : value;
    }
}
