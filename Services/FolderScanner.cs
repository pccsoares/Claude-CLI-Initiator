using System.IO;
using ClaudeCliInitiator.Models;

namespace ClaudeCliInitiator.Services;

public class FolderScanner
{
    private readonly string _reposPath;

    public FolderScanner(string reposPath = @"C:\Repos")
    {
        _reposPath = reposPath;
    }

    public List<RepoFolder> GetFolders()
    {
        var folders = new List<RepoFolder>();

        if (!Directory.Exists(_reposPath))
        {
            return folders;
        }

        try
        {
            var directories = Directory.GetDirectories(_reposPath);
            foreach (var dir in directories)
            {
                var name = Path.GetFileName(dir);

                // Skip folders starting with . or named "cd"
                if (name.StartsWith(".") || name.Equals("cd", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                folders.Add(new RepoFolder
                {
                    Name = name,
                    FullPath = dir,
                    Score = 0
                });
            }
        }
        catch (UnauthorizedAccessException)
        {
            // Skip directories we can't access
        }
        catch (IOException)
        {
            // Handle IO errors gracefully
        }

        return folders;
    }
}
