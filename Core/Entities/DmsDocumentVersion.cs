namespace DMSMigration.Core.Entities;

public class DmsDocumentVersion
{
    public int Id { get; set; }
    public int No { get; set; } // Version number (NO kolonu)
    public string FileName { get; set; } = string.Empty;
    public string Extension { get; set; } = string.Empty;
    public string? Path { get; set; }
    public long Size { get; set; }
    public int DocumentId { get; set; }
    public DateTime CreationTime { get; set; }
    public long? CreatorUserId { get; set; }
    public DateTime? LastModificationTime { get; set; }
    public long? LastModifierUserId { get; set; }
    public int IsDeleted { get; set; }
    public long? DeleterUserId { get; set; }
    public DateTime? DeletionTime { get; set; }
    public int? TenantId { get; set; }

    public DmsDocument Document { get; set; } = null!;
}
