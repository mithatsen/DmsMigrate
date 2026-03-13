namespace DMSMigration.Core.Models;

public class DocumentTypeConfig
{
    public string FolderName { get; set; } = string.Empty;
    public string TypeKey { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;

    public string GetNormalizedFolderName()
    {
        // "Kredi Analiz Raporu" → "KrediAnalizRaporu"
        return FolderName.Replace(" ", "");
    }

    public string GetServiceName()
    {
        // "KOF" → "KofTemplateService"
        // "Kredi Analiz Raporu" → "KrediAnalizRaporuTemplateService"
        var normalized = GetNormalizedFolderName();
        return $"{normalized}TemplateService";
    }
}
