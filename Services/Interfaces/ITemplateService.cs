using DMSMigration.Core.Models;

namespace DMSMigration.Services.Interfaces;

public interface ITemplateService
{
    string FolderName { get; }
    bool CanHandle(string filePath);
    Task ProcessMetadataAsync(FileMetadata metadata);
}
