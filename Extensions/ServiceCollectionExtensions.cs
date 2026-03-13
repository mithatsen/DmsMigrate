using DMSMigration.Data.Repositories;
using DMSMigration.Infrastructure;
using DMSMigration.Services;
using DMSMigration.Services.Interfaces;
using DMSMigration.Services.Templates;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DMSMigration.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDmsMigrationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Add Dapper repository (performanslı DB işlemleri için)
        services.AddScoped<IDapperDocumentRepository, DapperDocumentRepository>();

        // Add core services
        services.AddScoped<IFileService, FileService>();
        services.AddScoped<IDocumentService, DocumentService>();
        services.AddScoped<IMigrationService, MigrationService>();

        // Add template services (convention-based registration)
        services.AddScoped<ITemplateService, KofTemplateService>();
        services.AddScoped<ITemplateService, KrediAnalizRaporuTemplateService>();
        services.AddScoped<ITemplateService, LimitOnayFormuTemplateService>();
        services.AddScoped<ITemplateService, DefaultTemplateService>();

        // Add state manager
        var stateFilePath = configuration["MigrationSettings:StateFilePath"] ?? "migration-state.json";
        services.AddSingleton(sp =>
            new MigrationStateManager(
                stateFilePath,
                sp.GetRequiredService<ILogger<MigrationStateManager>>()));

        return services;
    }
}
