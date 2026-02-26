using DMSMigration.Core.Models;
using DMSMigration.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace DMSMigration.Services;

public class FileService : IFileService
{
    private readonly ILogger<FileService> _logger;

    public FileService(ILogger<FileService> logger)
    {
        _logger = logger;
    }

    public async Task<FileMetadata> GetFileMetadataAsync(string filePath)
    {
        var fileInfo = new FileInfo(filePath);

        if (!fileInfo.Exists)
        {
            throw new FileNotFoundException($"Dosya bulunamadı: {filePath}");
        }

        return new FileMetadata
        {
            FilePath = Path.GetFileName(fileInfo.Name),  
            FileName = Path.GetFileNameWithoutExtension(fileInfo.Name),
            Extension = fileInfo.Extension.TrimStart('.'),
            Size = fileInfo.Length,
            CreationTime = DateTime.Now,
            LastModificationTime = DateTime.Now
        };
    }

    public async Task<string> CopyFileToTargetAsync(string sourceFilePath, string targetDirectory)
    {
        if (!Directory.Exists(targetDirectory))
        {
            Directory.CreateDirectory(targetDirectory);
        }

        var fileName = Path.GetFileName(sourceFilePath);
        var targetPath = Path.Combine(targetDirectory, fileName);

        // Handle duplicates by adding a unique suffix
        if (File.Exists(targetPath))
        {
            var nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            var extension = Path.GetExtension(fileName);
            var counter = 1;

            do
            {
                fileName = $"{nameWithoutExtension}_{counter}{extension}";
                targetPath = Path.Combine(targetDirectory, fileName);
                counter++;
            } while (File.Exists(targetPath));

            _logger.LogWarning("Duplicate dosya bulundu. Yeni adı: {FileName}", fileName);
        }

        await Task.Run(() => File.Copy(sourceFilePath, targetPath));
        _logger.LogDebug("Dosya kopyalandı: {Source} -> {Target}", sourceFilePath, targetPath);

        return targetPath;
    }

    public bool FileExists(string filePath)
    {
        return File.Exists(filePath);
    }

    public List<string> GetAllFiles(string directory, string[] supportedExtensions)
    {
        if (!Directory.Exists(directory))
        {
            _logger.LogWarning("Kaynak dizin mevcut değil: {Directory}", directory);
            return new List<string>();
        }

        var allFiles = Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories);
        var filteredFiles = allFiles
            .Where(f => supportedExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
            .ToList();

        _logger.LogInformation("{Directory} dizininde {Count} dosya bulundu", filteredFiles.Count, directory);
        return filteredFiles;
    }
}
