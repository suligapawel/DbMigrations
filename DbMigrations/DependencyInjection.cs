using System.Data;
using DbMigrations.Providers;
using DbMigrations.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DbMigrations;

public static class DependencyInjection
{
    public static IServiceCollection AddDbMigrations<TDbConnection, TDbProvider>(this IServiceCollection services, IConfiguration config,
        string settingsName)
        where TDbProvider : IDbProvider
    {
        var dbSettings = config.GetSection(settingsName).Get<DbSettings>();
        var dbConnection = (IDbConnection)Activator.CreateInstance(typeof(TDbConnection), dbSettings.ConnectionString);
        
        return services
            .AddTransient(_ => dbConnection)
            .AddTransient(_ => (IDbProvider)Activator.CreateInstance(typeof(TDbProvider), dbConnection, dbSettings));
    }

    public static async Task<IApplicationBuilder> UseDbMigrations(this IApplicationBuilder app)
    {
        var dbProvider = app.ApplicationServices.GetService<IDbProvider>();
        await dbProvider.CreateMigrationsTableIfDoesNotExist();

        return app;
    }
}