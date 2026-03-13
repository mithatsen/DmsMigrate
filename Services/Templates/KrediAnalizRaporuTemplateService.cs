using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace DMSMigration.Services.Templates;

public class KrediAnalizRaporuTemplateService : BaseTemplateService
{
    private static readonly Regex FileNameRegex = new(@"(\d{8,10})_", RegexOptions.Compiled);

    public override string FolderName => "Kredi Analiz Raporu";

    public KrediAnalizRaporuTemplateService(ILogger<KrediAnalizRaporuTemplateService> logger) : base(logger)
    {
    }

    protected override Task<Dictionary<string, string>> GetIndexesAsync(string fileName)
    {
        var indexes = new Dictionary<string, string>
        {
            ["DocumentType"] = "KrediAnalizRaporu"
        };

        // Dosya ismi pattern: 999888777_rapor.pdf
        var match = FileNameRegex.Match(fileName);
        if (match.Success)
        {
            var projectNo = match.Groups[1].Value;
            indexes["ProjectNo"] = projectNo;

            // TODO: ProjectNo ile proje bilgilerini çek
            // indexes["CustomerId"] = ...
            // indexes["ReportDate"] = ...
        }

        return Task.FromResult(indexes);
    }
}
