using DMSMigration.Core.Entities;

namespace DMSMigration.Data.Repositories;

public interface IDapperDocumentRepository
{
    Task<int> InsertDocumentAsync(DmsDocument document);
    Task InsertDocumentIndexesAsync(IEnumerable<DmsDocumentIndex> indexes);
    Task<bool> DocumentExistsAsync(string fileName);
    Task<DmsIndex?> GetIndexByKeyAsync(string key);
    Task<int> InsertIndexAsync(DmsIndex index);
    Task BulkInsertDocumentsAsync(IEnumerable<DmsDocument> documents);
    Task BulkInsertIndexesAsync(IEnumerable<DmsDocumentIndex> indexes);
}
