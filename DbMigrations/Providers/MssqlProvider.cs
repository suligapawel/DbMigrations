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
				IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{_settings.TableName}' AND TABLE_SCHEMA = '{_settings.Schema}')
				BEGIN 
					CREATE TABLE {TableNameWithSchema}
					(
						[Id] [int] IDENTITY(1,1) PRIMARY KEY NOT NULL,
						[CreatedAt] [datetime2](3) NOT NULL,
						[Name] [varchar](100) NOT NULL UNIQUE
					)
				END";

        await _dbConnectionProvider.ExecuteAsync(query);
    }

    public async Task InsertMigrationIfDoesNotExist(IMigration migration)
    {
        var addMigrationQuery = $@"
				INSERT INTO {TableNameWithSchema}
				(Name, CreatedAt)
				VALUES ('{GetMigrationName(migration)}','{migration.CreatedAt:s}')";

        _dbConnectionProvider.Open();
        using var transaction = _dbConnectionProvider.BeginTransaction();

        try
        {
            if (await MigrationExist(migration, transaction))
            {
                return;
            }

            await _dbConnectionProvider.ExecuteAsync(migration.Up(), transaction: transaction);
            await _dbConnectionProvider.ExecuteAsync(addMigrationQuery, transaction: transaction);

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
        }
        finally
        {
            _dbConnectionProvider.Close();
        }
    }

    private async Task<bool> MigrationExist(IMigration migration, IDbTransaction transaction)
    {
        var query = $@"
				IF EXISTS
					(SELECT TOP(1) 1 FROM {TableNameWithSchema}
					WHERE Name = '{GetMigrationName(migration)}')
				BEGIN SELECT 1 END
				ELSE BEGIN SELECT 0 END";

        return await _dbConnectionProvider.QueryFirstAsync<bool>(query, transaction: transaction);
    }

    private static string GetMigrationName(IMigration migration)
        => migration.GetType().Name;
}