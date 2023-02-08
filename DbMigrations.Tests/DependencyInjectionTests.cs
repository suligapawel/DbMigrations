using System.Data;
using System.Reflection;
using Dapper;
using DbMigrations.Providers;
using DbMigrations.Tests.Fixtures;
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
        const string tableName = "testTableName";
        var dbConnection = await PrepareConnection(tableName);

        var result = await dbConnection.QueryFirstAsync<bool>(
            $@"IF EXISTS
                    (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{tableName}' AND TABLE_SCHEMA = 'dbo')
                BEGIN 
                    DROP TABLE dbo.{tableName};
	                SELECT 1;
                END;");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task Should_migrate_scripts()
    {
        const string tableName = "testTableName";
        var testDbMigration = new TestMigration();
        var dbConnection = await PrepareConnection(tableName);

        var result = await dbConnection.QueryFirstAsync<bool>(
            $@"DROP TABLE dbo.{tableName};
                IF EXISTS
                    (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{TestMigration.TableName}' AND TABLE_SCHEMA = 'dbo')
                BEGIN 
                    {testDbMigration.DownScript()};
	                SELECT 1;
                END;");

        result.Should().BeTrue();
    }

    private async Task<IDbConnection> PrepareConnection(string tableName)
    {
        var dbSettings = new Dictionary<string, string>
        {
            { "dbMigrator:connectionString", "Server=localhost;Database=test;User Id=sa;Password=Admin123!;TrustServerCertificate=True" },
            { "dbMigrator:schema", "dbo" },
            { "dbMigrator:tableName", tableName },
        };
        var services = new ServiceCollection();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(dbSettings)
            .Build();
        services.AddDbMigrations<SqlConnection, MssqlProvider>(config, "dbMigrator", Assembly.GetExecutingAssembly());
        var applicationBuilder = new ApplicationBuilder(services.BuildServiceProvider());
        await applicationBuilder.UseDbMigrations();
        applicationBuilder.Build();

        return applicationBuilder.ApplicationServices.GetService<IDbConnection>();
    }
}