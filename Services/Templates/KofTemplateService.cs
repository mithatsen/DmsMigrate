using DMSMigration.Core.Models;
using DMSMigration.Data.Repositories;
using DMSMigration.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace DMSMigration.Services.Templates;

public class KofTemplateService : BaseTemplateService
{
    // Pattern: 25010796R0_IS_KOF.dot → ProjeNo: 25010796
    private static readonly Regex R0Regex = new(@"^(\d{8,10})R0_", RegexOptions.Compiled);

    private readonly IDapperDocumentRepository _repository;

    public override string FolderName => "KOF";

    public KofTemplateService(
        ILogger<KofTemplateService> logger,
        IDapperDocumentRepository repository) : base(logger)
    {
        _repository = repository;
    }

    public override bool CanHandle(string filePath)
    {
        // 1. Klasör kontrolü (KOF klasöründe mi?)
        if (!base.CanHandle(filePath))
            return false;

        // 2. Sadece R0 olanları işle
        var fileName = Path.GetFileName(filePath);
        return R0Regex.IsMatch(fileName);
    }

    protected override async Task<Dictionary<string, string>> GetIndexesAsync(string fileName)
    {
        var indexes = new Dictionary<string, string>{};

        // Dosya ismi pattern: 25010796R0_IS_KOF.dot
        var match = R0Regex.Match(fileName);
        if (!match.Success)
        {
            Logger.LogWarning("KOF dosya ismi formatı hatalı: {FileName}", fileName);
            return indexes;
        }

        var projeNo = match.Groups[1].Value;

        try
        {
            var projeDetay = await _repository.GetProjeDetayByProjeNoAsync(projeNo);

            if (projeDetay != null)
            {
                // 1: Kredi, 2: Teklif, 3: Musteri
                indexes["Kredi"] = projeDetay.KrediId.ToString();
                indexes["Teklif"] = projeDetay.TeklifId.ToString();
                indexes["Musteri"] = projeDetay.MusteriId.ToString();

                Logger.LogDebug("ProjeNo {ProjeNo} için index'ler eklendi: Kredi={Kredi}, Teklif={Teklif}, Musteri={Musteri}", 
                    projeNo, projeDetay.KrediId, projeDetay.TeklifId, projeDetay.MusteriId);
            }
            else
            {
                Logger.LogWarning("ProjeNo {ProjeNo} için proje detay bulunamadı, sadece ProjeNo index'lendi", projeNo);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "ProjeNo {ProjeNo} için DB sorgusu başarısız", projeNo);
        }

        return indexes;
    }
}
