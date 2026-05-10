using FluentMigrator;

namespace XPos.Data.Migrations;

[Migration(202605090018)]
public class M202605090018_AddCategoryDescription : Migration
{
    public override void Up()
    {
        Alter.Table("categories")
            .AddColumn("description").AsCustom("TEXT").Nullable();
    }

    public override void Down()
    {
        Delete.Column("description").FromTable("categories");
    }
}