using Microsoft.Extensions.Logging;

namespace DMSMigration.Extensions;

public static class LoggingExtensions
{
    public static ILoggingBuilder AddDmsMigrationLogging(this ILoggingBuilder logging)
    {
        // Tüm varsayılan logging'i temizle
        logging.ClearProviders();
        
        // Sadece file logging ekle
        logging.AddFile("Logs/migration-{Date}.txt");

        return logging;
    }
}
