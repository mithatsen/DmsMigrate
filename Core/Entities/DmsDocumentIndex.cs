namespace DMSMigration.Core.Entities;

public class DmsDocumentIndex
{
    public int Id { get; set; }
    public int DocumentId { get; set; }
    public string IndexKey { get; set; } = string.Empty;
    public string IndexValue { get; set; } = string.Empty;
    public DateTime CreationTime { get; set; }

    public DmsDocument Document { get; set; } = null!;
}
