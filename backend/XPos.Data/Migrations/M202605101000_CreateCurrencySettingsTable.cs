using FluentMigrator;

namespace XPos.Data.Migrations;

[Migration(202605101000)]
public class M202605101000_CreateCurrencySettingsTable : Migration
{
    public override void Up()
    {
        Create.Table("CurrencySettings")
            .WithColumn("Id").AsInt64().PrimaryKey().Identity()
            .WithColumn("Code").AsString(10).NotNullable()
            .WithColumn("Symbol").AsString(5).NotNullable();
    }

    public override void Down()
    {
        Delete.Table("CurrencySettings");
    }
}
