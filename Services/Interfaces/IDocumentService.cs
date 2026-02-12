using DMSMigration.Core.Entities;
using DMSMigration.Core.Models;

namespace DMSMigration.Services.Interfaces;

public interface IDocumentService
{
    Task<DmsDocument> CreateDocumentAsync(FileMetadata metadata, int? tenantId, long? creatorUserId);
    Task<DmsDocumentVersion> CreateDocumentVersionAsync(int documentId, FileMetadata metadata, long? creatorUserId);
    Task CreateDocumentIndexesAsync(int documentId, Dictionary<string, string> indexes);
    Task<bool> DocumentExistsAsync(string fileName);
}
