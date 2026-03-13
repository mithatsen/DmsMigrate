using DMSMigration.Core.Entities;
using DMSMigration.Core.Models;
using DMSMigration.Data.Repositories;
using DMSMigration.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace DMSMigration.Services;

public class DocumentService : IDocumentService
{
    private readonly IDapperDocumentRepository _dapperRepository;
    private readonly ILogger<DocumentService> _logger;
    private readonly Dictionary<string, int> _indexCache = new();
    private readonly Dictionary<string, int> _typeCache = new();

    public DocumentService(
        IDapperDocumentRepository dapperRepository,
        ILogger<DocumentService> logger)
    {
        _dapperRepository = dapperRepository;
        _logger = logger;
    }

    public async Task<DmsDocument> CreateDocumentAsync(FileMetadata metadata, int? tenantId, long? creatorUserId)
    {
        // TypeKey'den TypeId'yi resolve et
        var typeId = await GetOrCreateTypeIdAsync(metadata.TypeKey, tenantId, creatorUserId);

        var document = new DmsDocument
        {
            FileName = metadata.FileName,
            Extension = metadata.Extension,
            Path = metadata.FilePath,
            Size = metadata.Size,
            TypeId = typeId,
            CurrentVersion = 1,
            CreationTime = DateTime.UtcNow,
            CreatorUserId = creatorUserId,
            IsDeleted = 0,
            TenantId = tenantId
        };

        // Dapper kullanarak performanslı insert
        document.Id = await _dapperRepository.InsertDocumentAsync(document);

        _logger.LogDebug("Doküman oluşturuldu: {FileName} (ID: {Id}, TypeKey: {TypeKey})", 
            document.FileName, document.Id, metadata.TypeKey);
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

    public async Task<bool> DocumentExistsAsync(string fileName, string extension)
    {
        // Dapper kullanarak performanslı sorgu
        return await _dapperRepository.DocumentExistsAsync(fileName, extension);
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

    private async Task<int> GetOrCreateTypeIdAsync(string typeKey, int? tenantId, long? creatorUserId)
    {
        // Cache'den kontrol et
        if (_typeCache.TryGetValue(typeKey, out var cachedTypeId))
        {
            return cachedTypeId;
        }

        // DB'den kontrol et
        var typeId = await _dapperRepository.GetTypeIdByKeyAsync(typeKey);
        if (typeId.HasValue)
        {
            _typeCache[typeKey] = typeId.Value;
            return typeId.Value;
        }

        // Bulunamadıysa hata - DMS_TYPE manuel oluşturulmalı
        throw new InvalidOperationException(
            $"TypeKey '{typeKey}' için DMS_TYPE kaydı bulunamadı. Lütfen önce DMS_TYPE tablosuna bu kaydı ekleyin.");
    }

    public async Task<DmsDocumentVersion> CreateDocumentVersionAsync(DmsDocument document, int? tenantId, long? creatorUserId)
    {
        var version = new DmsDocumentVersion
        {
            No = document.CurrentVersion, // İlk versiyon için 1
            FileName = document.FileName,
            Extension = document.Extension,
            Path = document.Path,
            Size = document.Size,
            DocumentId = document.Id,
            CreationTime = DateTime.UtcNow,
            CreatorUserId = creatorUserId,
            IsDeleted = 0,
            TenantId = tenantId
        };

        // Dapper kullanarak performanslı insert
        version.Id = await _dapperRepository.InsertDocumentVersionAsync(version);

        _logger.LogDebug("Doküman versiyonu oluşturuldu: {FileName} (Version: {VersionNo}, DocumentId: {DocumentId})", 
            version.FileName, version.No, document.Id);

        return version;
    }
}
