using BuildJanitor.Models;

namespace BuildJanitor.Scanners;

[ScannerKey("nodejs")]
public class NodeJsScanner : IProjectScanner
{
    public string Name => "Node.js";
    public ArtifactType Type => ArtifactType.NodeJs;

    public IEnumerable<string> FindArtifactFolders(string rootPath)
    {
        var foundPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var packageJson in FileEnumerator.EnumerateFilesSafe(rootPath, "package.json"))
        {
            var projectDir = Path.GetDirectoryName(packageJson)!;
            var nodeModulesPath = Path.Combine(projectDir, "node_modules");

            if (Directory.Exists(nodeModulesPath) && foundPaths.Add(nodeModulesPath))
                yield return nodeModulesPath;
        }
    }
}
