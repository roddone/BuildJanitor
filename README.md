# BuildJanitor

A cross-platform CLI tool that scans for and removes build artifacts from development projects, helping reclaim disk space.

In full transparency, BuildJanitor was a side project to test Claude Code. I used (and abused of) Claude to create this project.

## Features

- **Multi-project support**: Scans for .NET (`bin/ & obj/`) and Node.js (`node_modules`) artifacts
- **Interactive UI**: Terminal-based interface with keyboard navigation
- **Parallel processing**: Fast scanning using multi-core parallelization
- **Size calculation**: Shows folder sizes sorted by largest first
- **Batch operations**: Select and delete multiple folders at once
- **Cross-platform**: Runs on Windows, Linux, and macOS

## Installation

### From Release

Download the standalone executable for your platform from the [Releases](../../releases) page:

- `BuildJanitor-win-x64.exe` (Windows)
- `BuildJanitor-linux-x64` (Linux)
- `BuildJanitor-osx-x64` (macOS)

### From Source

Requires [.NET 10.0 SDK](https://dotnet.microsoft.com/download).

```bash
git clone https://github.com/yourusername/BuildJanitor.git
cd BuildJanitor
dotnet build -c Release
```

## Usage

```bash
# Scan current directory with all scanners
BuildJanitor

# Scan a specific directory
BuildJanitor --path /home/user/projects

# Use only specific scanners
BuildJanitor --scanners dotnet
BuildJanitor --scanners nodejs
BuildJanitor --scanners dotnet,nodejs
```

### CLI Options

| Option | Description |
|--------|-------------|
| `-p, --path <PATH>` | Folder to scan (defaults to current directory) |
| `-s, --scanners <SCANNERS>` | Comma-separated list of scanners to use |

### Available Scanners

| Key | Detects | Artifacts |
|-----|---------|-----------|
| `dotnet` | `.csproj` files | `bin/`, `obj/` folders |
| `nodejs` | `package.json` files | `node_modules/` folders |

## Keyboard Controls

| Key | Action |
|-----|--------|
| `Up/Down` | Navigate list |
| `PageUp/PageDown` | Jump multiple items |
| `Home/End` | Jump to first/last item |
| `Space` | Toggle selection |
| `Ctrl+A` | Select all |
| `Ctrl+D` | Deselect all |
| `1` | Toggle all .NET artifacts |
| `2` | Toggle all Node.js artifacts |
| `Enter` | Delete selected folders |
| `Q` / `Escape` | Quit |

## Building

```bash
# Debug build
dotnet build

# Release build
dotnet build -c Release

# Run directly
dotnet run --project BuildJanitor.Cli

# Publish standalone executable
dotnet publish -c Release -r win-x64 --self-contained
```

## License

Apache
