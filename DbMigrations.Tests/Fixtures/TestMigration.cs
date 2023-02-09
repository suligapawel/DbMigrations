namespace DbMigrations.Tests.Fixtures;

public class TestMigration : IMigration
{
    public const string TableName = "FakeTableForTest";

    public DateTime CreatedAt => new(2023, 02, 09, 23, 07, 00);

    public string Up()
        => $@"CREATE TABLE [dbo].[{TableName}](
                [Id] [int] IDENTITY(1,1) PRIMARY KEY NOT NULL
            )";

    public string Down()
        => $"DROP TABLE [dbo].[{TableName}]";
}