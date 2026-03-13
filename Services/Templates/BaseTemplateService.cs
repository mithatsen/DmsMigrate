using DMSMigration.Core.Models;
using DMSMigration.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace DMSMigration.Services.Templates;

public abstract class BaseTemplateService : ITemplateService
{
    protected readonly ILogger Logger;

    public abstract string FolderName { get; }

    protected BaseTemplateService(ILogger logger)
    {
        Logger = logger;
    }

    public virtual bool CanHandle(string filePath)
    {
        // Convention: Dosya yolu klasör adını içeriyorsa bu servis handle eder
        var normalizedPath = filePath.Replace("\\", "/");
        return normalizedPath.Contains($"/{FolderName}/", StringComparison.OrdinalIgnoreCase);
    }

    public async Task ProcessMetadataAsync(FileMetadata metadata)
    {
        // Alt sınıflar kendi index extraction mantığını uygular
        var indexes = await GetIndexesAsync(metadata.FileName);

        foreach (var kvp in indexes)
        {
            metadata.Indexes[kvp.Key] = kvp.Value;
        }

        Logger.LogDebug("{FolderName} template uygulandı: {FileName}", FolderName, metadata.FileName);
    }

    protected abstract Task<Dictionary<string, string>> GetIndexesAsync(string fileName);
}
