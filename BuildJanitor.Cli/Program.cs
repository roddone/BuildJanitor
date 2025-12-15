using BuildJanitor.Scanners;
using BuildJanitor.UI;

string rootPath = @"C:\Users\roddone\source\repos";

Console.WriteLine($"Scanning for .NET and Node.js projects in: {rootPath}");
Console.WriteLine("Please wait...\n");

var scanner = new ArtifactScanner([
    new DotNetScanner(),
    new NodeJsScanner()
]);

var folders = scanner.Scan(rootPath);

var ui = new ConsoleUI(folders, scanner, rootPath);
ui.Run();
