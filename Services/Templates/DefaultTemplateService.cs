using DMSMigration.Core.Models;
using DMSMigration.Services.Interfaces;

namespace DMSMigration.Services.Templates;

public class DefaultTemplateService : ITemplateService
{
    public bool CanHandle(string fileName)
    {
        // Default template handles all files
        return true;
    }

    public void EnrichMetadata(FileMetadata metadata)
    {
        metadata.TypeId = 99;
        metadata.Indexes["FileName"] = metadata.FileName;
    }
}
