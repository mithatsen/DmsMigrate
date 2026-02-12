using DMSMigration.Core.Entities;
using DMSMigration.Core.Models;
using DMSMigration.Data;
using DMSMigration.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DMSMigration.Services;

public class DocumentService : IDocumentService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DocumentService> _logger;

    public DocumentService(ApplicationDbContext context, ILogger<DocumentService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<DmsDocument> CreateDocumentAsync(FileMetadata metadata, int? tenantId, long? creatorUserId)
    {
        var document = new DmsDocument
        {
            FileName = metadata.FileName,
            Extension = metadata.Extension,
            Path = metadata.FilePath,
            Size = metadata.Size,
            TypeId = metadata.TypeId,
            CurrentVersion = 1,
            CreationTime = DateTime.Now,
            CreatorUserId = creatorUserId,
            IsDeleted = false,
            TenantId = tenantId
        };

        _context.DmsDocuments.Add(document);
        await _context.SaveChangesAsync();

        _logger.LogDebug("Document created: {FileName} (ID: {Id})", document.FileName, document.Id);
        return document;
    }

    public async Task<DmsDocumentVersion> CreateDocumentVersionAsync(int documentId, FileMetadata metadata, long? creatorUserId)
    {
        var version = new DmsDocumentVersion
        {
            DocumentId = documentId,
            VersionNumber = 1,
            FileName = metadata.FileName,
            Path = metadata.FilePath,
            Size = metadata.Size,
            CreationTime = DateTime.Now,
            CreatorUserId = creatorUserId
        };

        _context.DmsDocumentVersions.Add(version);
        await _context.SaveChangesAsync();

        _logger.LogDebug("Document version created: {FileName} (Version: {Version})", version.FileName, version.VersionNumber);
        return version;
    }

    public async Task CreateDocumentIndexesAsync(int documentId, Dictionary<string, string> indexes)
    {
        var indexEntities = indexes.Select(kvp => new DmsDocumentIndex
        {
            DocumentId = documentId,
            IndexKey = kvp.Key,
            IndexValue = kvp.Value,
            CreationTime = DateTime.Now
        }).ToList();

        _context.DmsDocumentIndexes.AddRange(indexEntities);
        await _context.SaveChangesAsync();

        _logger.LogDebug("Created {Count} indexes for document {DocumentId}", indexEntities.Count, documentId);
    }

    public async Task<bool> DocumentExistsAsync(string fileName)
    {
        return await _context.DmsDocuments.AnyAsync(d => d.FileName == fileName);
    }
}
