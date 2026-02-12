using DMSMigration.Core.Models;
using DMSMigration.Services.Interfaces;
using System.Text.RegularExpressions;

namespace DMSMigration.Services.Templates;

public class KofTemplateService : ITemplateService
{
    private static readonly Regex KofRegex = new(@"KOF_(\d{8})", RegexOptions.Compiled);

    public bool CanHandle(string fileName)
    {
        return fileName.StartsWith("KOF_", StringComparison.OrdinalIgnoreCase);
    }

    public void EnrichMetadata(FileMetadata metadata)
    {
        metadata.TypeId = 1;

        var match = KofRegex.Match(metadata.FileName);
        if (match.Success)
        {
            var projectNo = match.Groups[1].Value;
            metadata.Indexes["ProjectNo"] = projectNo;
            metadata.Indexes["DocumentType"] = "KOF";
        }
        else
        {
            // If pattern doesn't match, still add basic indexes
            metadata.Indexes["DocumentType"] = "KOF";
        }
    }
}
