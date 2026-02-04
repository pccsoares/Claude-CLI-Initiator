using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ClaudeCliInitiator.Services;

public class TerminalLauncher
{
    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool SetWindowText(IntPtr hWnd, string lpString);

    [DllImport("user32.dll")]
    private static extern IntPtr FindWindow(string? lpClassName, string? lpWindowName);

    [DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    public void Launch(string folderPath, string folderName)
    {
        if (TryLaunchWindowsTerminal(folderPath, folderName))
        {
            return;
        }

        LaunchCmd(folderPath, folderName);
    }

    private bool TryLaunchWindowsTerminal(string folderPath, string folderName)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "wt",
                Arguments = $"--title \"{folderName}\" -d \"{folderPath}\" cmd /k claude",
                UseShellExecute = true
            };
            Process.Start(startInfo);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private void RenameNewestTerminalWindow(string title)
    {
        try
        {
            // Find Windows Terminal processes
            var terminalProcesses = Process.GetProcessesByName("WindowsTerminal");
            if (terminalProcesses.Length == 0) return;

            // Get the most recently started one
            var newest = terminalProcesses
                .OrderByDescending(p => p.StartTime)
                .FirstOrDefault();

            if (newest?.MainWindowHandle != IntPtr.Zero)
            {
                SetWindowText(newest.MainWindowHandle, title);
            }
        }
        catch
        {
            // Silently fail
        }
    }

    private void LaunchCmd(string folderPath, string folderName)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "cmd",
                Arguments = $"/k \"cd /d \"{folderPath}\" && title {folderName} && claude\"",
                UseShellExecute = true
            };
            Process.Start(startInfo);
        }
        catch
        {
            // If cmd also fails, there's not much we can do
        }
    }
}
