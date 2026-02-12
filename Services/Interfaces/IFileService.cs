using DMSMigration.Core.Models;

namespace DMSMigration.Services.Interfaces;

public interface IFileService
{
    Task<FileMetadata> GetFileMetadataAsync(string filePath);
    Task<string> CopyFileToTargetAsync(string sourceFilePath, string targetDirectory);
    bool FileExists(string filePath);
    List<string> GetAllFiles(string directory, string[] supportedExtensions);
}
