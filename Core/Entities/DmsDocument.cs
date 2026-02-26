namespace DMSMigration.Core.Entities;

public class DmsDocument
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string Extension { get; set; } = string.Empty;
    public string? Path { get; set; }
    public long Size { get; set; }
    public int TypeId { get; set; }
    public int CurrentVersion { get; set; }
    public DateTime CreationTime { get; set; }
    public long? CreatorUserId { get; set; }
    public DateTime? LastModificationTime { get; set; }
    public long? LastModifierUserId { get; set; }
    public int IsDeleted { get; set; }
    public long? DeleterUserId { get; set; }
    public DateTime? DeletionTime { get; set; }
    public int? TenantId { get; set; }

    public ICollection<DmsDocumentIndex> Indexes { get; set; } = new List<DmsDocumentIndex>();
}
