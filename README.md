# Claude CLI Initiator

A Windows WPF application that helps navigate projects in `C:\Repos` and quickly launch Claude Code CLI.

<img src="https://raw.githubusercontent.com/pccsoares/Claude-CLI-Initiator/main/screenshot.png" width="400">

## Features

- **Scan & List**: Scans `C:\Repos` for first-level subfolders
- **Sorted Display**: Shows folders sorted by usage score (most used on top)
- **Search**: Filter folders by name as you type
- **Launch Action**: Double-click or press Enter to open Windows Terminal with Claude CLI
- **Score Tracking**: Persists click counts to prioritize frequently used projects
- **Window Title**: Sets the terminal tab title to the project name

## Requirements

- .NET 9.0
- Windows Terminal (falls back to cmd if not available)

## Windows Terminal Setup

For the tab title feature to work, add `suppressApplicationTitle` to your Windows Terminal settings:

**File:** `%LOCALAPPDATA%\Packages\Microsoft.WindowsTerminal_8wekyb3d8bbwe\LocalState\settings.json`

```json
{
    "profiles": {
        "defaults": {
            "suppressApplicationTitle": true
        }
    }
}
```

This prevents Claude from overriding the tab title set by the launcher.

## Build & Run

```bash
dotnet build
dotnet run
```

## Usage

1. Launch the application
2. Use the search box to filter projects (focused on startup)
3. Double-click a project or select and press Enter
4. Windows Terminal opens with Claude CLI in the project directory
5. The terminal tab is named after the project

## Data Storage

- **Scores**: `C:\Repos\claude-cli-scores.json`
- **Backup**: `C:\Repos\claude-cli-scores.json.bak`

## Project Structure

```
Claude-CLI-Initiator/
├── ClaudeCliInitiator.csproj
├── App.xaml / App.xaml.cs
├── MainWindow.xaml / MainWindow.xaml.cs
├── Models/
│   └── RepoFolder.cs
└── Services/
    ├── FolderScanner.cs
    ├── ScoreManager.cs
    └── TerminalLauncher.cs
```
