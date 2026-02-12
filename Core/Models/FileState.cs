using DMSMigration.Core.Enums;

namespace DMSMigration.Core.Models;

public class FileState
{
    public string FilePath { get; set; } = string.Empty;
    public MigrationStatus Status { get; set; }
    public DateTime LastUpdated { get; set; }
    public string? ErrorMessage { get; set; }
    public int RetryCount { get; set; }
}
