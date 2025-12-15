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

    Console.WriteLine($"Scanning for .NET and Node.js projects in: {rootPath}");
    Console.WriteLine("Please wait...\n");

    var scanner = new ArtifactScanner([
        new DotNetScanner(),
        new NodeJsScanner()
    ]);

    var folders = scanner.Scan(rootPath);

    var ui = new ConsoleUI(folders, scanner, rootPath);
    ui.Run();
}
