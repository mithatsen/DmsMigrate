namespace DMSMigration.Core.Entities;

public class DmsDocumentType
{
    public int Id { get; set; }
    public string? Key { get; set; }
    public string? Name { get; set; }
    public int IsSystem { get; set; }
    public int HasMultipleDocument { get; set; }
    public int? ParentId { get; set; }
    public DateTime CreationTime { get; set; }
    public long? CreatorUserId { get; set; }
    public DateTime? LastModificationTime { get; set; }
    public long? LastModifierUserId { get; set; }
    public int IsDeleted { get; set; }
    public long? DeleterUserId { get; set; }
    public DateTime? DeletionTime { get; set; }
    public int? TenantId { get; set; }
    public int IsDeletable { get; set; }

    public ICollection<DmsDocument> Documents { get; set; } = new List<DmsDocument>();
}
