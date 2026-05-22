using FluentMigrator;

namespace XPos.Data.Migrations;

[Migration(202605220002)]
public class M202605220002_AddAllWarehousesAccessToUsers : Migration
{
    public override void Up()
    {
        if (!Schema.Table("users").Column("all_warehouses_access").Exists())
        {
            Alter.Table("users")
                .AddColumn("all_warehouses_access").AsBoolean().WithDefaultValue(true);
        }
    }

    public override void Down()
    {
        if (Schema.Table("users").Column("all_warehouses_access").Exists())
        {
            Delete.Column("all_warehouses_access").FromTable("users");
        }
    }
}
