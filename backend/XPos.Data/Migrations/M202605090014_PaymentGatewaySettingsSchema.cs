using FluentMigrator;

namespace XPos.Data.Migrations;

[Migration(202605090014)]
public class M202605090014_PaymentGatewaySettingsSchema : Migration
{
    public override void Up()
    {
        Create.Table("payment_gateway_settings")
            .WithColumn("id").AsInt64().PrimaryKey().Identity()
            .WithColumn("gateway_name").AsString(100).NotNullable()
            .WithColumn("api_key").AsString(255).NotNullable() // Store encrypted in production
            .WithColumn("api_secret").AsString(255).Nullable() // Store encrypted in production
            .WithColumn("is_active").AsBoolean().WithDefaultValue(false)
            .WithColumn("description").AsString(500).Nullable()
            .WithColumn("created_at").AsDateTime().Nullable()
            .WithColumn("updated_at").AsDateTime().Nullable();
    }

    public override void Down()
    {
        Delete.Table("payment_gateway_settings");
    }
}
