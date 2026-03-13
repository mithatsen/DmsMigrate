using DMSMigration.Core.Models;
using DMSMigration.Services.Interfaces;

namespace DMSMigration.UI;

public static class MigrationConsole
{
    public static async Task<MigrationResult> RunInteractiveAsync(IMigrationService migrationService)
    {
        PrintMenu();
        
        var choice = Console.ReadLine();

        return choice switch
        {
            "1" => await migrationService.StartFromBeginningAsync(),
            "2" => await migrationService.RetryFailedAsync(),
            "3" => await migrationService.ResumeAsync(),
            _ => throw new InvalidOperationException("Geçersiz seçim")
        };
    }

    public static void PrintMenu()
    {
        Console.WriteLine("=== DMS Dosya Migration Uygulaması ===");
        Console.WriteLine("1. Sıfırdan başlat");
        Console.WriteLine("2. Hatalıları tekrar çalıştır");
        Console.WriteLine("3. Kaldığı yerden devam et");
        Console.WriteLine();
        Console.Write("Seçiminiz: ");
    }

    public static void PrintResults(MigrationResult result)
    {
        Console.WriteLine();
        Console.WriteLine("===========================================");
        Console.WriteLine("  MIGRATION SONUÇLARI");
        Console.WriteLine("===========================================");
        Console.WriteLine($"[+] Başarılı : {result.SuccessCount}");
        Console.WriteLine($"[-] Hatalı  : {result.FailedCount}");

        if (result.SkippedCount > 0)
        {
            Console.WriteLine($"[~] Atlanan : {result.SkippedCount}");
        }

        Console.WriteLine($"Toplam Süre: {result.Duration:hh\\:mm\\:ss}");
        Console.WriteLine("===========================================");

        if (result.Errors.Any())
        {
            Console.WriteLine();
            Console.WriteLine("Hatalar:");
            foreach (var error in result.Errors.Take(10))
            {
                Console.WriteLine($"  - {error}");
            }

            if (result.Errors.Count > 10)
            {
                Console.WriteLine($"  ... ve {result.Errors.Count - 10} hata daha");
            }
        }
    }
}
