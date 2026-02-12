using DMSMigration.Core.Models;

namespace DMSMigration.Services.Interfaces;

public interface IMigrationService
{
    Task<MigrationResult> StartFromBeginningAsync();
    Task<MigrationResult> RetryFailedAsync();
    Task<MigrationResult> ResumeAsync();
}
