using CommandLine;

namespace BuildJanitor;

public class Options
{
    [Option('p', "path", Required = false, HelpText = "Chemin du dossier à scanner. Utilise le dossier courant si non spécifié.")]
    public string? Path { get; set; }

    [Option('s', "scanners", Required = false, Separator = ',', HelpText = "Liste des scanners à utiliser (séparés par des virgules). Ex: dotnet,nodejs. Utilise tous les scanners si non spécifié.")]
    public IEnumerable<string>? Scanners { get; set; }
}
