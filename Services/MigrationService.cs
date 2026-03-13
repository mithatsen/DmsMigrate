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
    private readonly PerformanceMonitor _performanceMonitor;

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
        _performanceMonitor = new PerformanceMonitor();
    }

    public async Task<MigrationResult> StartFromBeginningAsync()
    {
        _logger.LogInformation("Migration baştan başlatılıyor...");
        _stateManager.Reset();

        var settings = _configuration.GetSection("MigrationSettings").Get<MigrationSettings>()
            ?? throw new InvalidOperationException("MigrationSettings yapılandırılmamış");

        // Enabled document type'ları al
        var enabledTypes = settings.DocumentTypes.Where(dt => dt.Enabled).ToList();

        if (enabledTypes.Count == 0)
        {
            throw new InvalidOperationException("Aktif document type bulunamadı");
        }

        // Tüm klasörlerden dosyaları topla
        var allFiles = new List<string>();

        foreach (var docType in enabledTypes)
        {
            var sourcePath = Path.Combine(settings.SourceBasePath, docType.FolderName);

            if (!Directory.Exists(sourcePath))
            {
                _logger.LogWarning("{FolderName} klasörü bulunamadı: {Path}", docType.FolderName, sourcePath);
                continue;
            }

            var files = _fileService.GetAllFiles(sourcePath, settings.SupportedExtensions);
            allFiles.AddRange(files);

            _logger.LogInformation("{FolderName}: {Count} dosya bulundu", docType.FolderName, files.Count);
        }

        _stateManager.InitializeFiles(allFiles);

        return await ExecuteMigrationAsync(allFiles);
    }

    public async Task<MigrationResult> RetryFailedAsync()
    {
        _logger.LogInformation("Hatalı dosyalar yeniden deneniyor...");
        
        var failedFiles = _stateManager.GetFailedFiles();
        var filePaths = failedFiles.Select(f => f.FilePath).ToList();

        _logger.LogInformation("Yeniden denenecek {Count} hatalı dosya bulundu", filePaths.Count);

        return await ExecuteMigrationAsync(filePaths);
    }

    public async Task<MigrationResult> ResumeAsync()
    {
        _logger.LogInformation("Migration kaldığı yerden devam ettiriliyor...");
        
        var pendingFiles = _stateManager.GetPendingFiles();
        var filePaths = pendingFiles.Select(f => f.FilePath).ToList();

        _logger.LogInformation("İşlenecek {Count} bekleyen dosya bulundu", filePaths.Count);

        return await ExecuteMigrationAsync(filePaths);
    }

    private async Task<MigrationResult> ExecuteMigrationAsync(List<string> files)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = new MigrationResult
        {
            TotalFiles = files.Count
        };

        var settings = _configuration.GetSection("MigrationSettings").Get<MigrationSettings>()
            ?? throw new InvalidOperationException("MigrationSettings yapılandırılmamış");

        Console.WriteLine();
        Console.WriteLine("===========================================");
        Console.WriteLine("  DMS MIGRATION BAŞLATILIYOR");
        Console.WriteLine("===========================================");
        Console.WriteLine($"Kaynak      : {settings.SourceBasePath}");
        Console.WriteLine($"Hedef       : {settings.TargetBasePath}");
        Console.WriteLine($"Toplam Dosya: {files.Count:N0}");
        Console.WriteLine($"Batch Boyutu: {settings.BatchSize}");
        Console.WriteLine();
        Console.WriteLine("Aktif Document Type'lar:");
        foreach (var docType in settings.DocumentTypes.Where(dt => dt.Enabled))
        {
            Console.WriteLine($"  - {docType.FolderName} (TypeKey: {docType.TypeKey})");
        }
        Console.WriteLine("===========================================");
        Console.WriteLine();

        _logger.LogInformation("DMS Migration başlatılıyor...");
        _logger.LogInformation("Toplam {TotalFiles} dosya bulundu, {ProcessCount} dosya işlenecek", 
            files.Count, files.Count);

        // Performance monitoring başlat
        _performanceMonitor.Start(files.Count);

        for (int i = 0; i < files.Count; i += settings.BatchSize)
        {
            var batch = files.Skip(i).Take(settings.BatchSize).ToList();

            _performanceMonitor.StartBatch();
            await ProcessBatchAsync(batch, settings, result);
            _performanceMonitor.EndBatch(batch.Count);

            // Her 5 batch'te bir ilerleme raporu (console'a)
            if ((i / settings.BatchSize) % 5 == 0 || i + settings.BatchSize >= files.Count)
            {
                _performanceMonitor.PrintProgress();
            }

            // Her batch sonrası log (sadece log dosyasına)
            _logger.LogInformation("İlerleme: {Processed}/{Total} dosya işlendi", 
                Math.Min(i + settings.BatchSize, files.Count), files.Count);
        }

        stopwatch.Stop();
        result.Duration = stopwatch.Elapsed;
        _performanceMonitor.Stop();

        // Final özet (console'a)
        Console.WriteLine();
        Console.WriteLine("===========================================");
        Console.WriteLine("  MIGRATION TAMAMLANDI");
        Console.WriteLine("===========================================");
        _performanceMonitor.PrintProgress();

        _logger.LogInformation("Migration tamamlandı, süre: {Duration}", result.Duration);
        return result;
    }

    private async Task ProcessBatchAsync(
        List<string> batch, 
        MigrationSettings settings,
        MigrationResult result)
    {
        foreach (var filePath in batch)
        {
            try
            {
                // Check retry count
                var state = _stateManager.GetFileState(filePath);
                if (state != null && state.RetryCount >= settings.MaxRetryCount)
                {
                    _logger.LogWarning("{FilePath} için maksimum deneme sayısına ulaşıldı. Atlanıyor.", filePath);
                    _stateManager.UpdateFileStatus(filePath, MigrationStatus.Skipped, "Maksimum deneme sayısı aşıldı");
                    result.SkippedCount++;
                    _performanceMonitor.RecordSkippedFile();
                    continue;
                }

                _stateManager.UpdateFileStatus(filePath, MigrationStatus.Processing);

                // Step 1: Get metadata
                var metadata = await _fileService.GetFileMetadataAsync(filePath);

                // Step 2: Dosyanın hangi klasöre ait olduğunu bul
                var docType = GetDocumentTypeFromPath(filePath, settings);
                if (docType == null)
                {
                    _logger.LogWarning("Dosya için document type bulunamadı: {FilePath}", filePath);
                    result.SkippedCount++;
                    _performanceMonitor.RecordSkippedFile();
                    continue;
                }

                metadata.TypeKey = docType.TypeKey;

                // Step 3: Enrich with template service
                var templateService = GetTemplateServiceByFolderName(docType.FolderName);
                if (templateService != null)
                {
                    // CanHandle kontrolü - R0 gibi özel koşullar burada devreye girer
                    if (!templateService.CanHandle(filePath))
                    {
                        _logger.LogDebug("Dosya template service tarafından handle edilmiyor (örn: R0 değil): {FilePath}", filePath);
                        result.SkippedCount++;
                        _stateManager.UpdateFileStatus(filePath, MigrationStatus.Skipped, "Template service koşulları sağlamıyor");
                        _performanceMonitor.RecordSkippedFile();
                        continue;
                    }

                    await templateService.ProcessMetadataAsync(metadata);
                }
                else
                {
                    // Fallback: TypeKey'i set et (template bulunamadıysa)
                    metadata.TypeKey = docType.TypeKey;
                }

                // Step 4: Döküman zaten varsa skip et
                // FILE_NAME + EXTENSION kombinasyonu ile kontrol et
                if (await _documentService.DocumentExistsAsync(metadata.FileName, metadata.Extension))
                {
                    _logger.LogDebug("Döküman zaten mevcut, atlanıyor: {FileName}.{Extension}", metadata.FileName, metadata.Extension);
                    result.SkippedCount++;
                    _stateManager.UpdateFileStatus(filePath, MigrationStatus.Skipped, "Döküman zaten mevcut");
                    _performanceMonitor.RecordSkippedFile();
                    continue;
                }

                // Step 5: Target path'i belirle (klasör bazlı)
                var targetFolder = Path.Combine(settings.TargetBasePath, docType.GetNormalizedFolderName());
                if (!Directory.Exists(targetFolder))
                {
                    Directory.CreateDirectory(targetFolder);
                }

                // Step 6: Copy file to target
                var targetFilePath = await _fileService.CopyFileToTargetAsync(filePath, targetFolder);

                metadata.FilePath = Path.GetFileName(targetFilePath);
                metadata.FileName = Path.GetFileNameWithoutExtension(targetFilePath);

                // Step 7: Create document record
                var document = await _documentService.CreateDocumentAsync(metadata, settings.DefaultTenantId, settings.DefaultCreatorUserId);

                // Step 8: Create initial document version (Version 1)
                await _documentService.CreateDocumentVersionAsync(document, settings.DefaultTenantId, settings.DefaultCreatorUserId);

                // Step 9: Create index records
                await _documentService.CreateDocumentIndexesAsync(document.Id, metadata.Indexes, settings.DefaultTenantId, settings.DefaultCreatorUserId);

                // Step 10: Update state
                _stateManager.UpdateFileStatus(filePath, MigrationStatus.Success);
                result.SuccessCount++;

                // Performance kaydı
                _performanceMonitor.RecordFile(metadata.Size);

                _logger.LogInformation("[OK] Başarılı: {FileName} (ID: {Id}, Type: {Type})", 
                    metadata.FileName, document.Id, docType.FolderName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[HATA] {FilePath}", filePath);
                _stateManager.UpdateFileStatus(filePath, MigrationStatus.Failed, ex.Message);
                result.FailedCount++;
                result.Errors.Add($"{filePath}: {ex.Message}");

                // Hatalı dosya da "işlendi" sayılır
                _performanceMonitor.RecordSkippedFile();
            }
        }
    }

    private DocumentTypeConfig? GetDocumentTypeFromPath(string filePath, MigrationSettings settings)
    {
        // Dosya path'inden klasör adını çıkar
        // C:\Source\KOF\file.pdf → "KOF"
        var normalizedPath = filePath.Replace("\\", "/");

        foreach (var docType in settings.DocumentTypes.Where(dt => dt.Enabled))
        {
            if (normalizedPath.Contains($"/{docType.FolderName}/", StringComparison.OrdinalIgnoreCase))
            {
                return docType;
            }
        }

        return null;
    }

    private ITemplateService? GetTemplateServiceByFolderName(string folderName)
    {
        // Convention: FolderName ile eşleşen servisi bul
        return _templateServices.FirstOrDefault(s => 
            s.FolderName.Equals(folderName, StringComparison.OrdinalIgnoreCase));
    }
}
