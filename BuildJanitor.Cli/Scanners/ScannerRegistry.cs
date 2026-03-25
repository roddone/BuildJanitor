namespace BuildJanitor.Scanners;

public static class ScannerRegistry
{
    private static readonly Dictionary<string, Func<IProjectScanner>> _scanners = new(StringComparer.OrdinalIgnoreCase)
    {
        ["dotnet"] = () => new DotNetScanner(),
        ["nodejs"] = () => new NodeJsScanner(),
        ["flutter"] = () => new FlutterScanner()
    };

    public static IEnumerable<string> AvailableKeys => _scanners.Keys;

    public static IProjectScanner? CreateScanner(string key)
    {
        if (_scanners.TryGetValue(key, out var factory))
        {
            return factory();
        }
        return null;
    }

    public static IEnumerable<IProjectScanner> CreateScanners(IEnumerable<string>? keys = null)
    {
        var targetKeys = keys ?? _scanners.Keys;

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
