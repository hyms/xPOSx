using FluentMigrator;

namespace XPos.Data.Migrations;

[Migration(202605090016)]
public class M202605090016_AddProductImage : Migration
{
    public override void Up()
    {
        Alter.Table("products")
            .AddColumn("image").AsCustom("TEXT").Nullable();
    }

    public override void Down()
    {
        Delete.Column("image").FromTable("products");
    }
}