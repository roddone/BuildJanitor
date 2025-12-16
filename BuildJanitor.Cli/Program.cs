using System.CommandLine;
using BuildJanitor.Scanners;
using BuildJanitor.UI;

var pathOption = new Option<string?>(
    aliases: ["-p", "--path"],
    description: "Chemin du dossier à scanner. Utilise le dossier courant si non spécifié.");

var scannersOption = new Option<string[]?>(
    aliases: ["-s", "--scanners"],
    description: "Liste des scanners à utiliser (séparés par des virgules). Ex: -s dotnet,nodejs. Utilise tous les scanners si non spécifié.")
{
    AllowMultipleArgumentsPerToken = true
};

var rootCommand = new RootCommand("BuildJanitor - Supprime les artefacts de build (.NET bin/obj, node_modules)")
{
    pathOption,
    scannersOption
};

rootCommand.SetHandler(Run, pathOption, scannersOption);

return await rootCommand.InvokeAsync(args);

void Run(string? path, string[]? scannerKeys)
{
    string rootPath = path ?? Environment.CurrentDirectory;

    if (!Directory.Exists(rootPath))
    {
        Console.WriteLine($"Erreur: Le dossier '{rootPath}' n'existe pas.");
        return;
    }

    var scanners = GetScanners(scannerKeys).ToList();

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

IEnumerable<IProjectScanner> GetScanners(string[]? keys)
{
    if (keys == null || keys.Length == 0)
    {
        return ScannerRegistry.CreateAllScanners();
    }

    var allKeys = keys.SelectMany(k => k.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

    var scanners = new List<IProjectScanner>();
    foreach (var key in allKeys)
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
