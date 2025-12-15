using System.Reflection;

namespace BuildJanitor.Scanners;

public static class ScannerRegistry
{
    private static readonly Dictionary<string, Type> _scanners = [];

    static ScannerRegistry()
    {
        DiscoverScanners();
    }

    private static void DiscoverScanners()
    {
        var scannerTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(IProjectScanner).IsAssignableFrom(t));

        foreach (var type in scannerTypes)
        {
            var keyAttr = type.GetCustomAttribute<ScannerKeyAttribute>();
            if (keyAttr != null)
            {
                _scanners[keyAttr.Key.ToLowerInvariant()] = type;
            }
        }
    }

    public static IEnumerable<string> AvailableKeys => _scanners.Keys;

    public static IProjectScanner? CreateScanner(string key)
    {
        if (_scanners.TryGetValue(key.ToLowerInvariant(), out var type))
        {
            return Activator.CreateInstance(type) as IProjectScanner;
        }
        return null;
    }

    public static IEnumerable<IProjectScanner> CreateScanners(IEnumerable<string>? keys = null)
    {
        var targetKeys = keys?.Select(k => k.ToLowerInvariant()) ?? _scanners.Keys;

        foreach (var key in targetKeys)
        {
            var scanner = CreateScanner(key);
            if (scanner != null)
            {
                yield return scanner;
            }
        }
    }

    public static IEnumerable<IProjectScanner> CreateAllScanners()
    {
        return CreateScanners(null);
    }
}
