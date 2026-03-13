using DMSMigration.Extensions;
using DMSMigration.Services.Interfaces;
using DMSMigration.UI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


try
{
    var builder = Host.CreateApplicationBuilder(args);

    builder.Services.AddDmsMigrationServices(builder.Configuration);
    builder.Logging.AddDmsMigrationLogging();

    var app = builder.Build();

    using var scope = app.Services.CreateScope();
    var migrationService = scope.ServiceProvider.GetRequiredService<IMigrationService>();

    var result = await MigrationConsole.RunInteractiveAsync(migrationService);
    MigrationConsole.PrintResults(result);

    return 0;
}
catch (Exception ex)
{
    Console.WriteLine($"Kritik hata: {ex.Message}");
    Console.WriteLine(ex.StackTrace);
    return 1;
}

