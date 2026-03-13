using DMSMigration.Core.Models;
using DMSMigration.Services.Templates;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace DMSMigration.Services.Templates;

public class LimitOnayFormuTemplateService : BaseTemplateService
{
    private static readonly Regex FileNameRegex = new(@"(\d{8,10})_", RegexOptions.Compiled);

    public override string FolderName => "Limit Onay Formu";

    public LimitOnayFormuTemplateService(ILogger<LimitOnayFormuTemplateService> logger) : base(logger)
    {
    }

    protected override Task<Dictionary<string, string>> GetIndexesAsync(string fileName)
    {
        var indexes = new Dictionary<string, string>
        {
            ["DocumentType"] = "LimitOnayFormu"
        };

        // Dosya ismi pattern: 123456_onay.xlsx
        var match = FileNameRegex.Match(fileName);
        if (match.Success)
        {
            var projectNo = match.Groups[1].Value;
            indexes["ProjectNo"] = projectNo;

            // TODO: ProjectNo ile proje bilgilerini çek
            // indexes["CustomerId"] = ...
            // indexes["LimitAmount"] = ...
        }

        return Task.FromResult(indexes);
    }
}
