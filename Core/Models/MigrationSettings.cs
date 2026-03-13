namespace DMSMigration.Core.Models;

public class MigrationSettings
{
    public string SourceBasePath { get; set; } = string.Empty;
    public string TargetBasePath { get; set; } = string.Empty;
    public List<DocumentTypeConfig> DocumentTypes { get; set; } = new();
    public int BatchSize { get; set; } = 100;
    public int MaxRetryCount { get; set; } = 3;
    public string StateFilePath { get; set; } = "migration-state.json";
    public string ErrorLogPath { get; set; } = "migration-errors.log";
    public string[] SupportedExtensions { get; set; } = Array.Empty<string>();
    public int? DefaultTenantId { get; set; }
    public long? DefaultCreatorUserId { get; set; }
    public bool UseDapper { get; set; } = true;
    public bool ParallelProcessing { get; set; } = false;
    public int MaxDegreeOfParallelism { get; set; } = 4;
}
