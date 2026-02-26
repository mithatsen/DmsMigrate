using DMSMigration.Core.Entities;
using DMSMigration.Core.Models;
using DMSMigration.Data;
using DMSMigration.Data.Repositories;
using DMSMigration.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DMSMigration.Services;

public class DocumentService : IDocumentService
{
    private readonly ApplicationDbContext _context;
    private readonly IDapperDocumentRepository _dapperRepository;
    private readonly ILogger<DocumentService> _logger;
    private readonly Dictionary<string, int> _indexCache = new();

    public DocumentService(
        ApplicationDbContext context,
        IDapperDocumentRepository dapperRepository,
        ILogger<DocumentService> logger)
    {
        _context = context;
        _dapperRepository = dapperRepository;
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
            CreationTime = DateTime.UtcNow,
            CreatorUserId = creatorUserId,
            IsDeleted = 0,
            TenantId = tenantId
        };

        // Dapper kullanarak performanslı insert
        document.Id = await _dapperRepository.InsertDocumentAsync(document);

        _logger.LogDebug("Doküman oluşturuldu: {FileName} (ID: {Id})", document.FileName, document.Id);
        return document;
    }

    public async Task CreateDocumentIndexesAsync(int documentId, Dictionary<string, string> indexes, int? tenantId, long? creatorUserId)
    {
        if (!indexes.Any()) return;

        var indexEntities = new List<DmsDocumentIndex>();

        foreach (var kvp in indexes)
        {
            // Index tanımını bul veya oluştur
            var indexId = await GetOrCreateIndexDefinitionAsync(kvp.Key, tenantId, creatorUserId);

            indexEntities.Add(new DmsDocumentIndex
            {
                DocumentId = documentId,
                IndexId = indexId,
                Value = kvp.Value,
                CreationTime = DateTime.UtcNow,
                CreatorUserId = creatorUserId,
                IsDeleted = 0,
                TenantId = tenantId
            });
        }

        // Dapper kullanarak performanslı bulk insert
        await _dapperRepository.InsertDocumentIndexesAsync(indexEntities);

        _logger.LogDebug("Doküman için {Count} index oluşturuldu: {DocumentId}", indexEntities.Count, documentId);
    }

    public async Task<bool> DocumentExistsAsync(string fileName)
    {
        // Dapper kullanarak performanslı sorgu
        return await _dapperRepository.DocumentExistsAsync(fileName);
    }

    public async Task<int> GetOrCreateIndexDefinitionAsync(string key, int? tenantId, long? creatorUserId)
    {
        // Cache'den kontrol et
        if (_indexCache.TryGetValue(key, out var cachedId))
        {
            return cachedId;
        }

        // DB'den kontrol et
        var existingIndex = await _dapperRepository.GetIndexByKeyAsync(key);
        if (existingIndex != null)
        {
            _indexCache[key] = existingIndex.Id;
            return existingIndex.Id;
        }

        // Yeni index tanımı oluştur
        var newIndex = new DmsIndex
        {
            Key = key,
            CreationTime = DateTime.UtcNow,
            CreatorUserId = creatorUserId,
            IsDeleted = 0,
            TenantId = tenantId
        };

        var indexId = await _dapperRepository.InsertIndexAsync(newIndex);
        _indexCache[key] = indexId;

        _logger.LogDebug("Yeni index tanımı oluşturuldu: {Key} (ID: {Id})", key, indexId);
        return indexId;
    }
}
