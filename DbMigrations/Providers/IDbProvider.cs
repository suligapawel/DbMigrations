namespace DbMigrations.Providers;

public interface IDbProvider
{
    Task CreateMigrationsTableIfDoesNotExist();
    Task InsertMigrationIfDoesNotExist(IMigration migration);
}