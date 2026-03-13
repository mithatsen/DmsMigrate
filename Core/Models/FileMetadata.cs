namespace DMSMigration.Core.Models;

public class FileMetadata
{
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string Extension { get; set; } = string.Empty;
    public long Size { get; set; }
    public DateTime CreationTime { get; set; }
    public DateTime LastModificationTime { get; set; }
    public string TypeKey { get; set; } = string.Empty;
    public int TypeId { get; set; }
    public Dictionary<string, string> Indexes { get; set; } = new();
}
