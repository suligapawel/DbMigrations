namespace DbMigrations;

public interface IMigration
{
    string UpScript();
    string DownScript();
}