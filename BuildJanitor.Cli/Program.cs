using BuildJanitor;
using BuildJanitor.Scanners;
using BuildJanitor.UI;
using CommandLine;

Parser.Default.ParseArguments<Options>(args)
    .WithParsed(Run);

void Run(Options options)
{
    string rootPath = options.Path ?? Environment.CurrentDirectory;

    if (!Directory.Exists(rootPath))
    {
        Console.WriteLine($"Erreur: Le dossier '{rootPath}' n'existe pas.");
        return;
    }

    var scanners = GetScanners(options.Scanners).ToList();

    if (scanners.Count == 0)
    {
        Console.WriteLine("Erreur: Aucun scanner valide spécifié.");
        Console.WriteLine($"Scanners disponibles: {string.Join(", ", ScannerRegistry.AvailableKeys)}");
        return;
    }

    var scannerNames = string.Join(", ", scanners.Select(s => s.Name));
    Console.WriteLine($"Scanning for {scannerNames} projects in: {rootPath}");
    Console.WriteLine("Please wait...\n");

    var scanner = new ArtifactScanner(scanners);
    var folders = scanner.Scan(rootPath);

    var ui = new ConsoleUI(folders, scanner, rootPath);
    ui.Run();
}

IEnumerable<IProjectScanner> GetScanners(IEnumerable<string>? keys)
{
    if (keys == null || !keys.Any())
    {
        return ScannerRegistry.CreateAllScanners();
    }

    var scanners = new List<IProjectScanner>();
    foreach (var key in keys)
    {
        var scanner = ScannerRegistry.CreateScanner(key);
        if (scanner != null)
        {
            scanners.Add(scanner);
        }
        else
        {
            Console.WriteLine($"Attention: Scanner '{key}' inconnu, ignoré.");
        }
    }
    return scanners;
}
