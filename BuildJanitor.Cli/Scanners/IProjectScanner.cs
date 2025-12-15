using BuildJanitor.Models;

namespace BuildJanitor.Scanners;

public interface IProjectScanner
{
    string Name { get; }
    ArtifactType Type { get; }
    IEnumerable<string> FindArtifactFolders(string rootPath);
}
