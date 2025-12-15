namespace BuildJanitor.Scanners;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class ScannerKeyAttribute(string key) : Attribute
{
    public string Key { get; } = key;
}
