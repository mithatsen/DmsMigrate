using DMSMigration.Core.Models;

namespace DMSMigration.Services.Interfaces;

public interface ITemplateService
{
    bool CanHandle(string fileName);
    void EnrichMetadata(FileMetadata metadata);
}
