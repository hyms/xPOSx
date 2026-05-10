using FluentMigrator;

namespace XPos.Data.Migrations;

[Migration(202605090001)]
public class M202605090001_CoreSchema : Migration
{
    public override void Up()
    {
        Create.Table("settings")
            .WithColumn("id").AsInt64().PrimaryKey().Identity()
            .WithColumn("company_name").AsString(255).NotNullable()
            .WithColumn("email").AsString(255).Nullable()
            .WithColumn("company_phone").AsString(255).Nullable()
            .WithColumn("company_address").AsString(255).Nullable()
            .WithColumn("logo").AsString(255).Nullable()
            .WithColumn("version").AsString(50).WithDefaultValue("1.0.0")
            .WithColumn("days").AsInt32().NotNullable()
            .WithColumn("created_at").AsDateTime().Nullable()
            .WithColumn("updated_at").AsDateTime().Nullable()
            .WithColumn("deleted_at").AsDateTime().Nullable();

        Create.Table("warehouses")
            .WithColumn("id").AsInt64().PrimaryKey().Identity()
            .WithColumn("name").AsString(255).NotNullable()
            .WithColumn("city").AsString(255).Nullable()
            .WithColumn("mobile").AsString(255).Nullable()
            .WithColumn("email").AsString(255).Nullable()
            .WithColumn("country").AsString(255).Nullable()
            .WithColumn("created_at").AsDateTime().Nullable()
            .WithColumn("updated_at").AsDateTime().Nullable()
            .WithColumn("deleted_at").AsDateTime().Nullable();

        Create.Table("roles")
            .WithColumn("id").AsInt64().PrimaryKey().Identity()
            .WithColumn("name").AsString(255).NotNullable()
            .WithColumn("guard_name").AsString(255).NotNullable()
            .WithColumn("created_at").AsDateTime().Nullable()
            .WithColumn("updated_at").AsDateTime().Nullable();

        Create.Table("permissions")
            .WithColumn("id").AsInt64().PrimaryKey().Identity()
            .WithColumn("name").AsString(255).NotNullable()
            .WithColumn("guard_name").AsString(255).NotNullable()
            .WithColumn("created_at").AsDateTime().Nullable()
            .WithColumn("updated_at").AsDateTime().Nullable();

        Create.Table("role_has_permissions")
            .WithColumn("permission_id").AsInt64().ForeignKey("permissions", "id")
            .WithColumn("role_id").AsInt64().ForeignKey("roles", "id");

        Create.Table("users")
            .WithColumn("id").AsInt64().PrimaryKey().Identity()
            .WithColumn("username").AsString(255).Unique().NotNullable()
            .WithColumn("password").AsCustom("TEXT").NotNullable()
            .WithColumn("email").AsString(255).NotNullable()
            .WithColumn("first_name").AsString(255).NotNullable()
            .WithColumn("last_name").AsString(255).NotNullable()
            .WithColumn("phone").AsString(255).Nullable()
            .WithColumn("role").AsInt32().NotNullable()
            .WithColumn("is_active").AsBoolean().WithDefaultValue(true)
            .WithColumn("last_login").AsDateTime().Nullable()
            .WithColumn("created_at").AsDateTime().Nullable()
            .WithColumn("updated_at").AsDateTime().Nullable()
            .WithColumn("deleted_at").AsDateTime().Nullable();

        Create.Table("user_warehouse")
            .WithColumn("user_id").AsInt64().ForeignKey("users", "id")
            .WithColumn("warehouse_id").AsInt64().ForeignKey("warehouses", "id");
    }

    public override void Down()
    {
        Delete.Table("user_warehouse");
        Delete.Table("users");
        Delete.Table("role_has_permissions");
        Delete.Table("permissions");
        Delete.Table("roles");
        Delete.Table("warehouses");
        Delete.Table("settings");
    }
}
