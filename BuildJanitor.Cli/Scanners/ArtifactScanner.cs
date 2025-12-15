using System.Collections.Concurrent;
using BuildJanitor.Models;

namespace BuildJanitor.Scanners;

public class ArtifactScanner(IEnumerable<IProjectScanner> scanners)
{
    private readonly IProjectScanner[] _scanners = scanners.ToArray();

    public List<ArtifactFolder> Scan(string rootPath)
    {
        var foldersToProcess = new ConcurrentBag<(string Path, ArtifactType Type)>();

        // Step 1: Find all artifact folders in parallel
        Console.Write("Finding projects... ");

        Parallel.ForEach(_scanners, scanner =>
        {
            foreach (var folder in scanner.FindArtifactFolders(rootPath))
                foldersToProcess.Add((folder, scanner.Type));
        });

        Console.WriteLine($"{foldersToProcess.Count} folders found");

        // Step 2: Calculate sizes in parallel
        Console.Write("Calculating sizes... ");
        var artifactFolders = new ConcurrentBag<ArtifactFolder>();
        int processed = 0;
        int total = foldersToProcess.Count;

        Parallel.ForEach(foldersToProcess,
            new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
            folder =>
            {
                var size = GetDirectorySize(folder.Path);
                artifactFolders.Add(new ArtifactFolder(folder.Path, size, folder.Type));

                var current = Interlocked.Increment(ref processed);
                if (current % 50 == 0 || current == total)
                    Console.Write($"\rCalculating sizes... {current}/{total}   ");
            });

        Console.WriteLine("Done!");

        return artifactFolders.OrderByDescending(f => f.Size).ToList();
    }

    private static long GetDirectorySize(string path)
    {
        long size = 0;
        try
        {
            var options = new EnumerationOptions
            {
                RecurseSubdirectories = true,
                IgnoreInaccessible = true,
                AttributesToSkip = FileAttributes.ReparsePoint
            };

            foreach (var file in Directory.EnumerateFiles(path, "*", options))
            {
                try { size += new FileInfo(file).Length; }
                catch { }
            }
        }
        catch { }
        return size;
    }
}
