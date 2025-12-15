using CommandLine;

namespace BuildJanitor;

public class Options
{
    [Option('p', "path", Required = false, HelpText = "Chemin du dossier à scanner. Utilise le dossier courant si non spécifié.")]
    public string? Path { get; set; }
}
