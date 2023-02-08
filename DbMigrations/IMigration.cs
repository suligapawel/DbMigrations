namespace DbMigrations;

public interface IMigration
{
    Task Up();
    Task Down();
}