using BuildJanitor.Models;
using BuildJanitor.Scanners;

namespace BuildJanitor.UI;

public class ConsoleUI(List<ArtifactFolder> folders, ArtifactScanner scanner, string rootPath)
{
    private readonly ArtifactScanner _scanner = scanner;
    private readonly string _rootPath = rootPath;
    private List<ArtifactFolder> _folders = folders;
    private int _currentIndex;
    private int _scrollOffset;

    public void Run()
    {
        if (_folders.Count == 0)
        {
            Console.WriteLine("No artifact folders found.");
            return;
        }

        Console.CursorVisible = false;
        Console.Clear();

        while (true)
        {
            int visibleRows = Console.WindowHeight - 7;
            AdjustScroll(visibleRows);
            Draw(visibleRows);

            if (!HandleInput(visibleRows))
                break;
        }
    }

    private void AdjustScroll(int visibleRows)
    {
        if (_currentIndex < _scrollOffset)
            _scrollOffset = _currentIndex;
        else if (_currentIndex >= _scrollOffset + visibleRows)
            _scrollOffset = _currentIndex - visibleRows + 1;
    }

    private void Draw(int visibleRows)
    {
        Console.SetCursorPosition(0, 0);

        DrawHeader();
        DrawList(visibleRows);
        DrawFooter();
    }

    private void DrawHeader()
    {
        var selectedCount = _folders.Count(f => f.IsSelected);
        var selectedSize = _folders.Where(f => f.IsSelected).Sum(f => f.Size);
        var totalSize = _folders.Sum(f => f.Size);
        var dotnetCount = _folders.Count(f => f.Type == ArtifactType.DotNet);
        var nodeCount = _folders.Count(f => f.Type == ArtifactType.NodeJs);

        // Title bar
        Console.BackgroundColor = ConsoleColor.DarkBlue;
        Console.ForegroundColor = ConsoleColor.White;
        var header = $" Build Janitor - {_folders.Count} folders ({FormatSize(totalSize)}) | Selected: {selectedCount} ({FormatSize(selectedSize)}) ";
        Console.WriteLine(header.PadRight(Console.WindowWidth - 1));
        Console.ResetColor();

        // Stats
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write($" .NET: {dotnetCount}");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"  Node.js: {nodeCount}");
        Console.ResetColor();

        // Help
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine(" [↑↓] Navigate  [Space] Select  [Enter] Delete  [1] .NET  [2] Node  [Ctrl+A] All  [Q] Quit");
        Console.ResetColor();
        Console.WriteLine(new string('─', Console.WindowWidth - 1));
    }

    private void DrawList(int visibleRows)
    {
        int endIndex = Math.Min(_scrollOffset + visibleRows, _folders.Count);

        for (int i = _scrollOffset; i < endIndex; i++)
            DrawItem(_folders[i], i == _currentIndex);

        // Fill remaining rows
        for (int i = endIndex - _scrollOffset; i < visibleRows; i++)
            Console.WriteLine(new string(' ', Console.WindowWidth - 1));
    }

    private static void DrawItem(ArtifactFolder folder, bool isCurrent)
    {
        if (isCurrent)
        {
            Console.BackgroundColor = ConsoleColor.DarkGray;
            Console.ForegroundColor = ConsoleColor.White;
        }

        string checkbox = folder.IsSelected ? "[X]" : "[ ]";
        string typeIndicator = folder.Type == ArtifactType.DotNet ? "[.NET]" : "[Node]";
        string sizeStr = FormatSize(folder.Size).PadLeft(10);
        string pathStr = TruncatePath(folder.Path, Console.WindowWidth - 26);

        if (!isCurrent)
        {
            Console.Write($" {checkbox} ");
            Console.ForegroundColor = folder.Type == ArtifactType.DotNet ? ConsoleColor.Cyan : ConsoleColor.Green;
            Console.Write(typeIndicator);
            Console.ResetColor();
            Console.WriteLine($" {sizeStr}  {pathStr}".PadRight(Console.WindowWidth - 12));
        }
        else
        {
            Console.WriteLine($" {checkbox} {typeIndicator} {sizeStr}  {pathStr}".PadRight(Console.WindowWidth - 1));
            Console.ResetColor();
        }
    }

    private void DrawFooter()
    {
        Console.WriteLine(new string('─', Console.WindowWidth - 1));
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write($" {_currentIndex + 1}/{_folders.Count}");
        Console.ResetColor();
    }

    private bool HandleInput(int visibleRows)
    {
        var key = Console.ReadKey(true);

        switch (key.Key)
        {
            case ConsoleKey.UpArrow:
                if (_currentIndex > 0) _currentIndex--;
                break;

            case ConsoleKey.DownArrow:
                if (_currentIndex < _folders.Count - 1) _currentIndex++;
                break;

            case ConsoleKey.PageUp:
                _currentIndex = Math.Max(0, _currentIndex - visibleRows);
                break;

            case ConsoleKey.PageDown:
                _currentIndex = Math.Min(_folders.Count - 1, _currentIndex + visibleRows);
                break;

            case ConsoleKey.Home:
                _currentIndex = 0;
                break;

            case ConsoleKey.End:
                _currentIndex = _folders.Count - 1;
                break;

            case ConsoleKey.Spacebar:
                _folders[_currentIndex].IsSelected = !_folders[_currentIndex].IsSelected;
                if (_currentIndex < _folders.Count - 1) _currentIndex++;
                break;

            case ConsoleKey.A when key.Modifiers == ConsoleModifiers.Control:
                _folders.ForEach(f => f.IsSelected = true);
                break;

            case ConsoleKey.D when key.Modifiers == ConsoleModifiers.Control:
                _folders.ForEach(f => f.IsSelected = false);
                break;

            case ConsoleKey.D1 or ConsoleKey.NumPad1:
                ToggleSelection(ArtifactType.DotNet);
                break;

            case ConsoleKey.D2 or ConsoleKey.NumPad2:
                ToggleSelection(ArtifactType.NodeJs);
                break;

            case ConsoleKey.Enter:
                var selected = _folders.Where(f => f.IsSelected).ToList();
                if (selected.Count > 0)
                {
                    if (DeleteFoldersAndRescan(selected))
                    {
                        _currentIndex = Math.Min(_currentIndex, Math.Max(0, _folders.Count - 1));
                        _scrollOffset = 0;
                        if (_folders.Count == 0) return false;
                    }
                }
                break;

            case ConsoleKey.Escape or ConsoleKey.Q:
                Console.Clear();
                Console.CursorVisible = true;
                Console.WriteLine("Exiting without deleting.");
                return false;
        }

        return true;
    }

    private bool DeleteFoldersAndRescan(List<ArtifactFolder> selected)
    {
        Console.Clear();
        Console.CursorVisible = true;

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"\nAbout to delete {selected.Count} folders ({FormatSize(selected.Sum(f => f.Size))}):\n");
        Console.ResetColor();

        foreach (var folder in selected.Take(10))
            Console.WriteLine($"  - {folder.Path}");

        if (selected.Count > 10)
            Console.WriteLine($"  ... and {selected.Count - 10} more");

        Console.ForegroundColor = ConsoleColor.Red;
        Console.Write("\nConfirm deletion? (Y/n): ");
        Console.ResetColor();

        var response = Console.ReadLine();
        if (response?.Equals("n", StringComparison.OrdinalIgnoreCase) == true)
        {
            Console.CursorVisible = false;
            Console.Clear();
            return false;
        }

        int deleted = 0, errors = 0;

        foreach (var folder in selected)
        {
            try
            {
                Console.Write($"Deleting {folder.Path}... ");
                Directory.Delete(folder.Path, true);
                deleted++;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("OK");
            }
            catch (Exception ex)
            {
                errors++;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error: {ex.Message}");
            }
            Console.ResetColor();
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\nDeleted {deleted} folders. {errors} errors.");
        Console.ResetColor();

        // Rescan
        Console.WriteLine("\nRescanning...\n");
        _folders = _scanner.Scan(_rootPath);

        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey(true);

        Console.CursorVisible = false;
        Console.Clear();
        return true;
    }

    private void ToggleSelection(ArtifactType type)
    {
        var items = _folders.Where(f => f.Type == type).ToList();
        bool allSelected = items.All(f => f.IsSelected);
        items.ForEach(f => f.IsSelected = !allSelected);
    }

    private static string TruncatePath(string path, int maxLength)
    {
        if (path.Length <= maxLength)
            return path;
        return "..." + path[(path.Length - maxLength + 3)..];
    }

    private static string FormatSize(long bytes)
    {
        string[] suffixes = ["B", "KB", "MB", "GB", "TB"];
        int index = 0;
        double size = bytes;

        while (size >= 1024 && index < suffixes.Length - 1)
        {
            size /= 1024;
            index++;
        }

        return $"{size:0.##} {suffixes[index]}";
    }
}
