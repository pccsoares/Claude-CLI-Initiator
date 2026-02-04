using System.IO;
using System.Text.Json;

namespace ClaudeCliInitiator.Services;

public class ScoreManager
{
    private readonly string _scoresFilePath;
    private readonly string _backupFilePath;
    private Dictionary<string, int> _scores;

    public ScoreManager()
    {
        _scoresFilePath = @"C:\Repos\claude-cli-scores.json";
        _backupFilePath = @"C:\Repos\claude-cli-scores.json.bak";
        _scores = new Dictionary<string, int>();
        Load();
    }

    private void Load()
    {
        // Try main file first
        if (TryLoadFromFile(_scoresFilePath))
        {
            return;
        }

        // Fall back to backup
        if (TryLoadFromFile(_backupFilePath))
        {
            // Restore backup to main file
            Save();
            return;
        }

        _scores = new Dictionary<string, int>();
    }

    private bool TryLoadFromFile(string path)
    {
        if (!File.Exists(path))
        {
            return false;
        }

        try
        {
            var json = File.ReadAllText(path);
            var loaded = JsonSerializer.Deserialize<Dictionary<string, int>>(json);
            if (loaded != null)
            {
                _scores = loaded;
                return true;
            }
        }
        catch
        {
            // File is corrupted
        }

        return false;
    }

    private void Save()
    {
        try
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(_scores, options);

            // Backup existing file first
            if (File.Exists(_scoresFilePath))
            {
                File.Copy(_scoresFilePath, _backupFilePath, overwrite: true);
            }

            // Write to temp file first, then rename (atomic operation)
            var tempFile = _scoresFilePath + ".tmp";
            File.WriteAllText(tempFile, json);
            File.Move(tempFile, _scoresFilePath, overwrite: true);
        }
        catch
        {
            // Silently fail if we can't save
        }
    }

    public int GetScore(string folderName)
    {
        return _scores.TryGetValue(folderName, out var score) ? score : 0;
    }

    public void IncrementScore(string folderName)
    {
        if (_scores.ContainsKey(folderName))
        {
            _scores[folderName]++;
        }
        else
        {
            _scores[folderName] = 1;
        }
        Save();
    }

    public void ApplyScores(IEnumerable<Models.RepoFolder> folders)
    {
        foreach (var folder in folders)
        {
            folder.Score = GetScore(folder.Name);
        }
    }
}
