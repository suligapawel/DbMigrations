using System.Data;
using Dapper;
using DbMigrations.Settings;

namespace DbMigrations.Providers;

public class MssqlProvider : IDbProvider
{
    private readonly IDbConnection _dbConnection;
    private readonly DbSettings _settings;

    public MssqlProvider(IDbConnection dbConnection, DbSettings settings)
    {
        _dbConnection = dbConnection;
        _settings = settings;
    }

    public async Task CreateMigrationsTable()
    {
	    var query = $@"
				IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{_settings.TableName}' AND TABLE_SCHEMA = 'dbo')
				BEGIN 
					CREATE TABLE [{_settings.Schema}].[{_settings.TableName}]
					(
						[Id] [int] IDENTITY(1,1) PRIMARY KEY NOT NULL,
						[CreatedAt] [datetime2](3) NOT NULL CONSTRAINT DF___Migrations_CreatedAt DEFAULT GETDATE(),
						[Name] [varchar](100) NOT NULL 
					)
				END";
	    
        await _dbConnection.ExecuteAsync(query);
    }
}