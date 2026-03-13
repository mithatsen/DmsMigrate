using DMSMigration.Core.Entities;

namespace DMSMigration.Data.Repositories;

public interface IDapperDocumentRepository
{
    Task<int> InsertDocumentAsync(DmsDocument document);
    Task InsertDocumentIndexesAsync(IEnumerable<DmsDocumentIndex> indexes);
    Task<bool> DocumentExistsAsync(string fileName, string extension);
    Task<DmsIndex?> GetIndexByKeyAsync(string key);
    Task<int> InsertIndexAsync(DmsIndex index);
    Task BulkInsertDocumentsAsync(IEnumerable<DmsDocument> documents);
    Task BulkInsertIndexesAsync(IEnumerable<DmsDocumentIndex> indexes);
    Task<ProjeDetay?> GetProjeDetayByProjeNoAsync(string projeNo);
    Task<int?> GetTypeIdByKeyAsync(string typeKey);
    Task<int> InsertDocumentVersionAsync(DmsDocumentVersion version);
}

public class ProjeDetay
{
    public int ProjeId { get; set; }
    public int TeklifId { get; set; }
    public int MusteriId { get; set; }
    public int KrediId { get; set; }
}
