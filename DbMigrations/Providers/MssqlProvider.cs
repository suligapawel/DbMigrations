using System.Data;
using Dapper;
using DbMigrations.Settings;

namespace DbMigrations.Providers;

public class MssqlProvider : IDbProvider
{
    private string TableNameWithSchema => $"[{_settings.Schema}].[{_settings.TableName}]";

    private readonly DbSettings _settings;
    private readonly IDbConnection _dbConnectionProvider;

    public MssqlProvider(IDbConnection dbConnection, DbSettings settings)
    {
        _dbConnectionProvider = dbConnection;
        _settings = settings;
    }

    public async Task CreateMigrationsTableIfDoesNotExist()
    {
        var query = $@"
				IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{_settings.TableName}' AND TABLE_SCHEMA = 'dbo')
				BEGIN 
					CREATE TABLE {TableNameWithSchema}
					(
						[Id] [int] IDENTITY(1,1) PRIMARY KEY NOT NULL,
						[CreatedAt] [datetime2](3) NOT NULL CONSTRAINT DF___Migrations_CreatedAt DEFAULT GETDATE(),
						[Name] [varchar](100) NOT NULL UNIQUE
					)
				END";

        await _dbConnectionProvider.ExecuteAsync(query);
    }

    public async Task InsertMigrationInfo(string migrationName)
    {
        var query = $@"
				INSERT INTO {TableNameWithSchema}
				(Name)
				VALUES ('{migrationName}')";

        await _dbConnectionProvider.ExecuteAsync(query);
    }

    public async Task<bool> MigrationExist(string migrationName)
    {
        var query = $@"
				SELECT TOP(1) 1 FROM {TableNameWithSchema}
				WHERE Name = '{migrationName}'";

        return await _dbConnectionProvider.QueryFirstAsync<bool>(query);
    }
}