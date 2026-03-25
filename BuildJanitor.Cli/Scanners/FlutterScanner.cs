using BuildJanitor.Models;

namespace BuildJanitor.Scanners;

public class FlutterScanner : IProjectScanner
{
    public string Name => "Flutter";
    public ArtifactType Type => ArtifactType.Flutter;

    public IEnumerable<string> FindArtifactFolders(string rootPath)
    {
        foreach (var pubspecFile in FileEnumerator.EnumerateFilesSafe(rootPath, "pubspec.yaml"))
        {
            var projectDir = Path.GetDirectoryName(pubspecFile)!;

            var buildPath = Path.Combine(projectDir, "build");
            if (Directory.Exists(buildPath))
                yield return buildPath;

            var dartToolPath = Path.Combine(projectDir, ".dart_tool");
            if (Directory.Exists(dartToolPath))
                yield return dartToolPath;
        }
    }
}
