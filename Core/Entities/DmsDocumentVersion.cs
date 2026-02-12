namespace DMSMigration.Core.Entities;

public class DmsDocumentVersion
{
    public int Id { get; set; }
    public int DocumentId { get; set; }
    public int VersionNumber { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public long Size { get; set; }
    public DateTime CreationTime { get; set; }
    public long? CreatorUserId { get; set; }

    public DmsDocument Document { get; set; } = null!;
}
