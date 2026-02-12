using DMSMigration.Core.Enums;
using DMSMigration.Core.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace DMSMigration.Infrastructure;

public class MigrationStateManager
{
    private readonly string _stateFilePath;
    private readonly ILogger<MigrationStateManager> _logger;
    private Dictionary<string, FileState> _fileStates = new();

    public MigrationStateManager(string stateFilePath, ILogger<MigrationStateManager> logger)
    {
        _stateFilePath = stateFilePath;
        _logger = logger;
        LoadState();
    }

    public void UpdateFileStatus(string filePath, MigrationStatus status, string? errorMessage = null)
    {
        if (!_fileStates.ContainsKey(filePath))
        {
            _fileStates[filePath] = new FileState
            {
                FilePath = filePath,
                Status = status,
                LastUpdated = DateTime.UtcNow,
                ErrorMessage = errorMessage,
                RetryCount = 0
            };
        }
        else
        {
            var state = _fileStates[filePath];
            
            if (status == MigrationStatus.Failed)
            {
                state.RetryCount++;
            }
            
            state.Status = status;
            state.LastUpdated = DateTime.UtcNow;
            state.ErrorMessage = errorMessage;
        }

        SaveState();
    }

    public List<FileState> GetFailedFiles()
    {
        return _fileStates.Values
            .Where(fs => fs.Status == MigrationStatus.Failed)
            .ToList();
    }

    public List<FileState> GetPendingFiles()
    {
        return _fileStates.Values
            .Where(fs => fs.Status == MigrationStatus.Pending)
            .ToList();
    }

    public FileState? GetFileState(string filePath)
    {
        return _fileStates.TryGetValue(filePath, out var state) ? state : null;
    }

    public void SaveState()
    {
        try
        {
            var json = JsonSerializer.Serialize(_fileStates, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
            File.WriteAllText(_stateFilePath, json);
            _logger.LogDebug("State saved to {FilePath}", _stateFilePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save state to {FilePath}", _stateFilePath);
        }
    }

    public void LoadState()
    {
        try
        {
            if (File.Exists(_stateFilePath))
            {
                var json = File.ReadAllText(_stateFilePath);
                _fileStates = JsonSerializer.Deserialize<Dictionary<string, FileState>>(json) 
                    ?? new Dictionary<string, FileState>();
                _logger.LogInformation("State loaded from {FilePath}. {Count} files tracked.", 
                    _stateFilePath, _fileStates.Count);
            }
            else
            {
                _logger.LogInformation("No existing state file found. Starting fresh.");
                _fileStates = new Dictionary<string, FileState>();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load state from {FilePath}. Starting fresh.", _stateFilePath);
            _fileStates = new Dictionary<string, FileState>();
        }
    }

    public void Reset()
    {
        _fileStates.Clear();
        
        if (File.Exists(_stateFilePath))
        {
            File.Delete(_stateFilePath);
        }
        
        _logger.LogInformation("State reset.");
    }

    public void InitializeFiles(List<string> files)
    {
        foreach (var file in files)
        {
            if (!_fileStates.ContainsKey(file))
            {
                _fileStates[file] = new FileState
                {
                    FilePath = file,
                    Status = MigrationStatus.Pending,
                    LastUpdated = DateTime.UtcNow,
                    RetryCount = 0
                };
            }
        }
        SaveState();
    }
}
