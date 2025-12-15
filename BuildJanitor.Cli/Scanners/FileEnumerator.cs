namespace BuildJanitor.Scanners;

public static class FileEnumerator
{
    private static readonly HashSet<string> SkippedFolders = new(StringComparer.OrdinalIgnoreCase)
    {
        "node_modules",
        ".git"
    };

    public static IEnumerable<string> EnumerateFilesSafe(string rootPath, string pattern)
    {
        var directories = new Stack<string>();
        directories.Push(rootPath);

        while (directories.Count > 0)
        {
            var currentDir = directories.Pop();

            var dirName = Path.GetFileName(currentDir);
            if (SkippedFolders.Contains(dirName))
                continue;

            IEnumerable<string> files = [];
            try
            {
                files = Directory.EnumerateFiles(currentDir, pattern);
            }
            catch { }

            foreach (var file in files)
                yield return file;

            try
            {
                foreach (var subDir in Directory.EnumerateDirectories(currentDir))
                    directories.Push(subDir);
            }
            catch { }
        }
    }
}
