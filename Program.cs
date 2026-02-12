using DMSMigration.Data;
using DMSMigration.Infrastructure;
using DMSMigration.Services;
using DMSMigration.Services.Interfaces;
using DMSMigration.Services.Templates;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DMSMigration;

class Program
{
    static async Task<int> Main(string[] args)
    {
        try
        {
            // Build configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
                .Build();

            // Build host
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    // Add DbContext with Oracle
                    var connectionString = configuration.GetConnectionString("DefaultConnection");
                    services.AddDbContext<ApplicationDbContext>(options =>
                        options.UseOracle(connectionString));

                    // Add services
                    services.AddSingleton<IConfiguration>(configuration);
                    services.AddScoped<IFileService, FileService>();
                    services.AddScoped<IDocumentService, DocumentService>();
                    services.AddScoped<IMigrationService, MigrationService>();
                    
                    // Add template services (order matters - specific templates before default)
                    services.AddScoped<ITemplateService, KofTemplateService>();
                    services.AddScoped<ITemplateService, DefaultTemplateService>();

                    // Add state manager
                    var stateFilePath = configuration["MigrationSettings:StateFilePath"] ?? "migration-state.json";
                    services.AddSingleton(sp => 
                        new MigrationStateManager(
                            stateFilePath, 
                            sp.GetRequiredService<ILogger<MigrationStateManager>>()));

                    // Add logging
                    services.AddLogging(builder =>
                    {
                        builder.AddConsole();
                        builder.AddFile("Logs/migration-{Date}.txt");
                    });
                })
                .Build();

            // Run the application
            await RunApplicationAsync(host);

            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fatal error: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            return 1;
        }
    }

    static async Task RunApplicationAsync(IHost host)
    {
        using var scope = host.Services.CreateScope();
        var migrationService = scope.ServiceProvider.GetRequiredService<IMigrationService>();

        Console.WriteLine("=== DMS Dosya Migration Uygulaması ===");
        Console.WriteLine("1. Sıfırdan başlat");
        Console.WriteLine("2. Hatalıları tekrar çalıştır");
        Console.WriteLine("3. Kaldığı yerden devam et");
        Console.WriteLine();
        Console.Write("Seçiminiz: ");

        var choice = Console.ReadLine();

        var result = choice switch
        {
            "1" => await migrationService.StartFromBeginningAsync(),
            "2" => await migrationService.RetryFailedAsync(),
            "3" => await migrationService.ResumeAsync(),
            _ => throw new InvalidOperationException("Geçersiz seçim")
        };

        // Print results
        Console.WriteLine();
        Console.WriteLine("=== Migration Sonuçları ===");
        Console.WriteLine($"✓ Başarılı: {result.SuccessCount}");
        Console.WriteLine($"✗ Hatalı: {result.FailedCount}");
        
        if (result.SkippedCount > 0)
        {
            Console.WriteLine($"⊘ Atlanan: {result.SkippedCount}");
        }
        
        Console.WriteLine($"⏱ Süre: {result.Duration:hh\\:mm\\:ss}");

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
