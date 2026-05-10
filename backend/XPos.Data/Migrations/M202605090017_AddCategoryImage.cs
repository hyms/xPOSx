using FluentMigrator;

namespace XPos.Data.Migrations;

[Migration(202605090017)]
public class M202605090017_AddCategoryImage : Migration
{
    public override void Up()
    {
        Alter.Table("categories")
            .AddColumn("image").AsCustom("TEXT").Nullable();
    }

    public override void Down()
    {
        Delete.Column("image").FromTable("categories");
    }
}