using FluentMigrator;

namespace XPos.Data.Migrations;

[Migration(202605090012)]
public class M202605090012_CurrencySchema : Migration
{
    public override void Up()
    {
        Create.Table("currencies")
            .WithColumn("id").AsInt64().PrimaryKey().Identity()
            .WithColumn("code").AsString(10).NotNullable().Unique()
            .WithColumn("symbol").AsString(10).NotNullable()
            .WithColumn("name").AsString(50).NotNullable()
            .WithColumn("is_main").AsBoolean().WithDefaultValue(false);
    }

    public override void Down()
    {
        Delete.Table("currencies");
    }
}
