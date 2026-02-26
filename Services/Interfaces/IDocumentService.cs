using DMSMigration.Core.Entities;
using DMSMigration.Core.Models;

namespace DMSMigration.Services.Interfaces;

public interface IDocumentService
{
    Task<DmsDocument> CreateDocumentAsync(FileMetadata metadata, int? tenantId, long? creatorUserId);
    Task CreateDocumentIndexesAsync(int documentId, Dictionary<string, string> indexes, int? tenantId, long? creatorUserId);
    Task<bool> DocumentExistsAsync(string fileName);
    Task<int> GetOrCreateIndexDefinitionAsync(string key, int? tenantId, long? creatorUserId);
}
