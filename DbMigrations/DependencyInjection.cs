using System.Data;
using System.Reflection;
using DbMigrations.Providers;
using DbMigrations.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DbMigrations;

public static class DependencyInjection
{
    public static IServiceCollection AddDbMigrations<TDbConnection, TDbProvider>(
        this IServiceCollection services,
        IConfiguration config,
        string settingsName,
        params Assembly[] assemblies)
        where TDbProvider : IDbProvider
    {
        var dbSettings = config.GetSection(settingsName).Get<DbSettings>();
        var dbConnection = (IDbConnection)Activator.CreateInstance(typeof(TDbConnection), dbSettings.ConnectionString);

        services.Scan(x => x.FromAssemblies(assemblies)
            .AddClasses(c => c.AssignableTo<IMigration>().Where(t => !t.IsAbstract && !t.IsGenericTypeDefinition))
            .AsSelfWithInterfaces()
            .WithTransientLifetime());

        return services
            .AddTransient(_ => dbConnection)
            .AddTransient(_ => (IDbProvider)Activator.CreateInstance(typeof(TDbProvider), dbConnection, dbSettings));
    }

    public static async Task<IApplicationBuilder> UseDbMigrations(this IApplicationBuilder app)
    {
        var dbProvider = app.ApplicationServices.GetService<IDbProvider>();
        var migrations = app.ApplicationServices.GetServices<IMigration>();
        await dbProvider.CreateMigrationsTableIfDoesNotExist();
        
        foreach (var migration in migrations)
        {
            var migrationName = migration.GetType().Name;

            if (await dbProvider.MigrationExist(migrationName))
            {
                continue;
            }
            
            await migration.Up();
            await dbProvider.InsertMigrationInfo(migrationName);
        }

        return app;
    }
}