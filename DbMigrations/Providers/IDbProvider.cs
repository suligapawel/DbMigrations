namespace DbMigrations.Providers;

public interface IDbProvider
{
    Task CreateMigrationsTable();
}