using BuildJanitor.Models;

namespace BuildJanitor.Scanners;

public class DotNetScanner : IProjectScanner
{
    public string Name => ".NET";
    public ArtifactType Type => ArtifactType.DotNet;

    public IEnumerable<string> FindArtifactFolders(string rootPath)
    {
        foreach (var projectFile in FileEnumerator.EnumerateFilesSafe(rootPath, "*.csproj"))
        {
            var projectDir = Path.GetDirectoryName(projectFile)!;

            var binPath = Path.Combine(projectDir, "bin");
            if (Directory.Exists(binPath))
                yield return binPath;

            var objPath = Path.Combine(projectDir, "obj");
            if (Directory.Exists(objPath))
                yield return objPath;
        }
    }
}
