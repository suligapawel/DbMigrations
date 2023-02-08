using System.Data;
using Dapper;
using DbMigrations.Providers;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DbMigrations.Tests;

public class DependencyInjectionTests
{
    [Fact]
    public async Task Should_create_new_database_if_it_does_not_exist()
    {
        var dbSettings = new Dictionary<string, string>
        {
            { "dbMigrator:connectionString", "Server=localhost;Database=test;User Id=sa;Password=Admin123!;TrustServerCertificate=True" },
            { "dbMigrator:schema", "dbo" },
            { "dbMigrator:tableName", "testTableName" },
        };
        var services = new ServiceCollection();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(dbSettings)
            .Build();
        services.AddDbMigrations<SqlConnection, MssqlProvider>(config, "dbMigrator");
        var applicationBuilder = new ApplicationBuilder(services.BuildServiceProvider());
        await applicationBuilder.UseDbMigrations();
        applicationBuilder.Build();

        var dbConnection = applicationBuilder.ApplicationServices.GetService<IDbConnection>();

        var result = await dbConnection.QueryFirstAsync<bool>(
            $@"IF EXISTS
                    (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{dbSettings["dbMigrator:tableName"]}' AND TABLE_SCHEMA = 'dbo')
                BEGIN 
                    DROP TABLE dbo.{dbSettings["dbMigrator:tableName"]};
	                SELECT 1;
                END;");

        result.Should().BeTrue();
    }
}