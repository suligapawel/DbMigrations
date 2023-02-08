namespace DbMigrations.Settings;

public class DbSettings
{
    public string ConnectionString { get; init; }
    public string Schema { get; init; } = "dbo";
    public string TableName { get; init; } = "__Migrations";
}