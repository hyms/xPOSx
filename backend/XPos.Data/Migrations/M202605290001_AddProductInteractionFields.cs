using FluentMigrator;

namespace XPos.Data.Migrations;

[Migration(202605290001)]
public class M202605290001_AddProductInteractionFields : Migration
{
    public override void Up()
    {
        if (!Schema.Table("products").Column("is_featured").Exists())
        {
            Alter.Table("products")
                .AddColumn("is_featured").AsBoolean().NotNullable().WithDefaultValue(false);
        }

        if (!Schema.Table("products").Column("is_web_available").Exists())
        {
            Alter.Table("products")
                .AddColumn("is_web_available").AsBoolean().NotNullable().WithDefaultValue(true);
        }
    }

    public override void Down()
    {
        if (Schema.Table("products").Column("is_featured").Exists())
        {
            Delete.Column("is_featured").FromTable("products");
        }

        if (Schema.Table("products").Column("is_web_available").Exists())
        {
            Delete.Column("is_web_available").FromTable("products");
        }
    }
}
