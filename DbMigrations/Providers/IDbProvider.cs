namespace DbMigrations.Providers;

public interface IDbProvider
{
    Task CreateMigrationsTableIfDoesNotExist();
    Task InsertMigrationInfo(string migrationName);
    Task<bool> MigrationExist(string migrationName);
}