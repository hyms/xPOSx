using FluentMigrator;

namespace XPos.Data.Migrations;

[Migration(202605190001)]
public class M2026051901_IdentitySettings : Migration
{
    public override void Up()
    {
        Alter.Table("settings")
            .AddColumn("favicon").AsString(255).Nullable()
            .AddColumn("settings_version").AsInt32().WithDefaultValue(1);
    }

    public override void Down()
    {
        Delete.Column("favicon").FromTable("settings");
        Delete.Column("settings_version").FromTable("settings");
    }
}
