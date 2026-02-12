using DMSMigration.Core.Enums;
using DMSMigration.Core.Models;
using DMSMigration.Infrastructure;
using DMSMigration.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace DMSMigration.Services;

public class MigrationService : IMigrationService
{
    private readonly IFileService _fileService;
    private readonly IDocumentService _documentService;
    private readonly MigrationStateManager _stateManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<MigrationService> _logger;
    private readonly List<ITemplateService> _templateServices;

    public MigrationService(
        IFileService fileService,
        IDocumentService documentService,
        MigrationStateManager stateManager,
        IConfiguration configuration,
        ILogger<MigrationService> logger,
        IEnumerable<ITemplateService> templateServices)
    {
        _fileService = fileService;
        _documentService = documentService;
        _stateManager = stateManager;
        _configuration = configuration;
        _logger = logger;
        _templateServices = templateServices.ToList();
    }

    public async Task<MigrationResult> StartFromBeginningAsync()
    {
        _logger.LogInformation("Starting migration from beginning...");
        _stateManager.Reset();

        var sourcePath = _configuration["MigrationSettings:SourcePath"] 
            ?? throw new InvalidOperationException("SourcePath not configured");
        var supportedExtensions = _configuration.GetSection("MigrationSettings:SupportedExtensions").Get<string[]>() 
            ?? Array.Empty<string>();

        var files = _fileService.GetAllFiles(sourcePath, supportedExtensions);
        _stateManager.InitializeFiles(files);

        return await ExecuteMigrationAsync(files);
    }

    public async Task<MigrationResult> RetryFailedAsync()
    {
        _logger.LogInformation("Retrying failed migrations...");
        
        var failedFiles = _stateManager.GetFailedFiles();
        var filePaths = failedFiles.Select(f => f.FilePath).ToList();

        _logger.LogInformation("Found {Count} failed files to retry", filePaths.Count);

        return await ExecuteMigrationAsync(filePaths);
    }

    public async Task<MigrationResult> ResumeAsync()
    {
        _logger.LogInformation("Resuming migration from last checkpoint...");
        
        var pendingFiles = _stateManager.GetPendingFiles();
        var filePaths = pendingFiles.Select(f => f.FilePath).ToList();

        _logger.LogInformation("Found {Count} pending files to process", filePaths.Count);

        return await ExecuteMigrationAsync(filePaths);
    }

    private async Task<MigrationResult> ExecuteMigrationAsync(List<string> files)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = new MigrationResult
        {
            TotalFiles = files.Count
        };

        var batchSize = _configuration.GetValue<int>("MigrationSettings:BatchSize", 100);
        var targetPath = _configuration["MigrationSettings:TargetPath"] 
            ?? throw new InvalidOperationException("TargetPath not configured");
        var maxRetryCount = _configuration.GetValue<int>("MigrationSettings:MaxRetryCount", 3);
        var defaultTenantId = _configuration.GetValue<int?>("MigrationSettings:DefaultTenantId");
        var defaultCreatorUserId = _configuration.GetValue<long?>("MigrationSettings:DefaultCreatorUserId");

        _logger.LogInformation("DMS Migration başlatılıyor...");
        _logger.LogInformation("Toplam {TotalFiles} dosya bulundu, {ProcessCount} dosya işlenecek", 
            files.Count, files.Count);

        for (int i = 0; i < files.Count; i += batchSize)
        {
            var batch = files.Skip(i).Take(batchSize).ToList();
            await ProcessBatchAsync(batch, targetPath, maxRetryCount, defaultTenantId, defaultCreatorUserId, result);

            _logger.LogInformation("İlerleme: {Processed}/{Total} dosya işlendi", 
                Math.Min(i + batchSize, files.Count), files.Count);
        }

        stopwatch.Stop();
        result.Duration = stopwatch.Elapsed;

        _logger.LogInformation("Migration completed in {Duration}", result.Duration);
        return result;
    }

    private async Task ProcessBatchAsync(
        List<string> batch, 
        string targetPath, 
        int maxRetryCount,
        int? tenantId,
        long? creatorUserId,
        MigrationResult result)
    {
        foreach (var filePath in batch)
        {
            try
            {
                // Check retry count
                var state = _stateManager.GetFileState(filePath);
                if (state != null && state.RetryCount >= maxRetryCount)
                {
                    _logger.LogWarning("Max retry count reached for {FilePath}. Skipping.", filePath);
                    _stateManager.UpdateFileStatus(filePath, MigrationStatus.Skipped, "Max retry count exceeded");
                    result.SkippedCount++;
                    continue;
                }

                _stateManager.UpdateFileStatus(filePath, MigrationStatus.Processing);

                // Step 1: Get metadata
                var metadata = await _fileService.GetFileMetadataAsync(filePath);

                // Step 2: Enrich with template service
                var templateService = GetTemplateService(metadata.FileName);
                templateService.EnrichMetadata(metadata);

                // Step 3: Check for duplicates
                var exists = await _documentService.DocumentExistsAsync(metadata.FileName);
                if (exists)
                {
                    _logger.LogWarning("Duplicate document found: {FileName}. Continuing with unique name.", 
                        metadata.FileName);
                }

                // Step 4: Copy file to target
                var targetFilePath = await _fileService.CopyFileToTargetAsync(filePath, targetPath);
                metadata.FilePath = targetFilePath;

                // Step 5: Create document record
                var document = await _documentService.CreateDocumentAsync(metadata, tenantId, creatorUserId);

                // Step 6: Create version record
                await _documentService.CreateDocumentVersionAsync(document.Id, metadata, creatorUserId);

                // Step 7: Create index records
                await _documentService.CreateDocumentIndexesAsync(document.Id, metadata.Indexes);

                // Step 8: Update state
                _stateManager.UpdateFileStatus(filePath, MigrationStatus.Success);
                result.SuccessCount++;

                _logger.LogInformation("✓ Başarılı: {FileName} (ID: {Id})", metadata.FileName, document.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "✗ Hata: {FilePath}", filePath);
                _stateManager.UpdateFileStatus(filePath, MigrationStatus.Failed, ex.Message);
                result.FailedCount++;
                result.Errors.Add($"{filePath}: {ex.Message}");
            }
        }
    }

    private ITemplateService GetTemplateService(string fileName)
    {
        foreach (var service in _templateServices)
        {
            if (service.CanHandle(fileName))
            {
                return service;
            }
        }

        // Should never happen as DefaultTemplateService handles all files
        return _templateServices.Last();
    }
}
