namespace BuildJanitor.Models;

public enum ArtifactType { DotNet, NodeJs }

public class ArtifactFolder(string path, long size, ArtifactType type)
{
    public string Path { get; } = path;
    public long Size { get; } = size;
    public ArtifactType Type { get; } = type;
    public bool IsSelected { get; set; }
}
