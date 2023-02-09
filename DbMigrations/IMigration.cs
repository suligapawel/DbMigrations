namespace DbMigrations;

public interface IMigration
{
    DateTime CreatedAt { get; }
    
    string Up();
    string Down();
}