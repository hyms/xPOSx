using FluentMigrator;

namespace XPos.Data.Migrations;

[Migration(202605090013)]
public class M202605090013_MailSettingsSchema : Migration
{
    public override void Up()
    {
        Create.Table("mail_settings")
            .WithColumn("id").AsInt64().PrimaryKey().Identity()
            .WithColumn("host").AsString(255).NotNullable()
            .WithColumn("port").AsInt32().NotNullable()
            .WithColumn("username").AsString(255).NotNullable()
            .WithColumn("password").AsString(255).NotNullable() // Store encrypted in production
            .WithColumn("encryption").AsString(50).Nullable() // SSL, TLS, etc.
            .WithColumn("from_address").AsString(255).NotNullable()
            .WithColumn("from_name").AsString(255).Nullable()
            .WithColumn("created_at").AsDateTime().Nullable()
            .WithColumn("updated_at").AsDateTime().Nullable();
    }

    public override void Down()
    {
        Delete.Table("mail_settings");
    }
}
