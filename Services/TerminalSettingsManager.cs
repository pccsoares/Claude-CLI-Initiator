using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ClaudeCliInitiator.Services;

public class TerminalSettingsManager
{
    private readonly string _settingsPath;

    public TerminalSettingsManager()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        _settingsPath = Path.Combine(localAppData,
            "Packages",
            "Microsoft.WindowsTerminal_8wekyb3d8bbwe",
            "LocalState",
            "settings.json");
    }

    public bool SettingsFileExists => File.Exists(_settingsPath);

    public bool IsSuppressApplicationTitleEnabled()
    {
        if (!SettingsFileExists)
            return false;

        try
        {
            var json = File.ReadAllText(_settingsPath);
            var root = JsonNode.Parse(json);

            var suppressValue = root?["profiles"]?["defaults"]?["suppressApplicationTitle"];
            return suppressValue?.GetValue<bool>() == true;
        }
        catch
        {
            return false;
        }
    }

    public bool EnableSuppressApplicationTitle()
    {
        if (!SettingsFileExists)
            return false;

        try
        {
            var json = File.ReadAllText(_settingsPath);
            var root = JsonNode.Parse(json);

            if (root == null)
                return false;

            // Ensure profiles exists
            if (root["profiles"] == null)
                root["profiles"] = new JsonObject();

            // Ensure defaults exists
            if (root["profiles"]!["defaults"] == null)
                root["profiles"]!["defaults"] = new JsonObject();

            // Set suppressApplicationTitle
            root["profiles"]!["defaults"]!["suppressApplicationTitle"] = true;

            // Write back with formatting
            var options = new JsonSerializerOptions { WriteIndented = true };
            var newJson = root.ToJsonString(options);
            File.WriteAllText(_settingsPath, newJson);

            return true;
        }
        catch
        {
            return false;
        }
    }
}
