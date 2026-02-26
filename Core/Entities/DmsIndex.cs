namespace DMSMigration.Core.Entities;

public class DmsIndex
{
    public int Id { get; set; }
    public string? Key { get; set; }
    public DateTime CreationTime { get; set; }
    public long? CreatorUserId { get; set; }
    public DateTime? LastModificationTime { get; set; }
    public long? LastModifierUserId { get; set; }
    public int IsDeleted { get; set; }
    public long? DeleterUserId { get; set; }
    public DateTime? DeletionTime { get; set; }
    public int? TenantId { get; set; }

    public ICollection<DmsDocumentIndex> DocumentIndexes { get; set; } = new List<DmsDocumentIndex>();
}
