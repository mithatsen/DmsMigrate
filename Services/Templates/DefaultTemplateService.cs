using DMSMigration.Core.Models;
using DMSMigration.Services.Templates;
using Microsoft.Extensions.Logging;

namespace DMSMigration.Services.Templates;

public class DefaultTemplateService : BaseTemplateService
{
    public override string FolderName => "Default";

    public DefaultTemplateService(ILogger<DefaultTemplateService> logger) : base(logger)
    {
    }

    public override bool CanHandle(string filePath)
    {
        // Default template handles all files
        return true;
    }

    protected override Task<Dictionary<string, string>> GetIndexesAsync(string fileName)
    {
        return Task.FromResult(new Dictionary<string, string>
        {
            ["DocumentType"] = "Default",
            ["FileName"] = fileName
        });
    }
}
