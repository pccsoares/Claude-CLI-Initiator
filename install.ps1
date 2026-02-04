# Claude CLI Initiator Installer
# Run as Administrator

param(
    [switch]$Uninstall
)

$AppName = "Claude CLI Initiator"
$ExeName = "ClaudeCliInitiator.exe"
$InstallDir = "$env:LOCALAPPDATA\ClaudeCliInitiator"
$StartMenuDir = "$env:APPDATA\Microsoft\Windows\Start Menu\Programs"
$ShortcutPath = "$StartMenuDir\$AppName.lnk"
$DesktopShortcut = [Environment]::GetFolderPath('Desktop') + "\$AppName.lnk"
$SourceDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$PublishDir = "$SourceDir\bin\Release\net9.0-windows\win-x64\publish"

function Create-Shortcut {
    param($ShortcutPath, $TargetPath, $IconPath)

    $WshShell = New-Object -ComObject WScript.Shell
    $Shortcut = $WshShell.CreateShortcut($ShortcutPath)
    $Shortcut.TargetPath = $TargetPath
    $Shortcut.IconLocation = $IconPath
    $Shortcut.WorkingDirectory = Split-Path -Parent $TargetPath
    $Shortcut.Save()
}

function Pin-ToTaskbar {
    param($ShortcutPath)

    # Copy shortcut to taskbar folder
    $TaskbarDir = "$env:APPDATA\Microsoft\Internet Explorer\Quick Launch\User Pinned\TaskBar"
    if (Test-Path $TaskbarDir) {
        Copy-Item $ShortcutPath $TaskbarDir -Force
        Write-Host "Added to taskbar folder. You may need to manually pin it."
    }
}

if ($Uninstall) {
    Write-Host "Uninstalling $AppName..."

    # Remove shortcuts
    if (Test-Path $ShortcutPath) { Remove-Item $ShortcutPath -Force }
    if (Test-Path $DesktopShortcut) { Remove-Item $DesktopShortcut -Force }

    # Remove install directory
    if (Test-Path $InstallDir) { Remove-Item $InstallDir -Recurse -Force }

    Write-Host "Uninstall complete."
    exit 0
}

Write-Host "Installing $AppName..."

# Check if publish directory exists
if (-not (Test-Path "$PublishDir\$ExeName")) {
    Write-Host "Error: Published executable not found at $PublishDir"
    Write-Host "Please run 'dotnet publish -c Release' first."
    exit 1
}

# Create install directory
if (-not (Test-Path $InstallDir)) {
    New-Item -ItemType Directory -Path $InstallDir | Out-Null
}

# Copy files
Write-Host "Copying files to $InstallDir..."
Copy-Item "$PublishDir\*" $InstallDir -Recurse -Force

# Copy icon
if (Test-Path "$SourceDir\icon.ico") {
    Copy-Item "$SourceDir\icon.ico" $InstallDir -Force
}

$ExePath = "$InstallDir\$ExeName"
$IconPath = "$InstallDir\icon.ico"

# Create Start Menu shortcut
Write-Host "Creating Start Menu shortcut..."
Create-Shortcut -ShortcutPath $ShortcutPath -TargetPath $ExePath -IconPath $IconPath

# Create Desktop shortcut
Write-Host "Creating Desktop shortcut..."
Create-Shortcut -ShortcutPath $DesktopShortcut -TargetPath $ExePath -IconPath $IconPath

# Pin to taskbar
Write-Host "Adding to taskbar..."
Pin-ToTaskbar -ShortcutPath $ShortcutPath

Write-Host ""
Write-Host "Installation complete!"
Write-Host "  Installed to: $InstallDir"
Write-Host "  Start Menu: $ShortcutPath"
Write-Host "  Desktop: $DesktopShortcut"
Write-Host ""
Write-Host "To pin to taskbar: Right-click the desktop shortcut and select 'Pin to taskbar'"
