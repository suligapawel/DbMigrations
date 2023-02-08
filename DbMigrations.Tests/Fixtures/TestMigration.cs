namespace DbMigrations.Tests.Fixtures;

public class TestMigration : IMigration
{
    public const string TableName = "FakeTableForTest";
    
    public string UpScript()
        => $@"CREATE TABLE [dbo].[{TableName}](
                [Id] [int] IDENTITY(1,1) PRIMARY KEY NOT NULL
            )";

    public string DownScript()
        => $"DROP TABLE [dbo].[{TableName}]";
}