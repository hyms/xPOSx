using FluentMigrator;

namespace XPos.Data.Migrations;

[Migration(202605090003)]
public class M202605090003_ContactSchema : Migration
{
    public override void Up()
    {
        Create.Table("clients")
            .WithColumn("id").AsInt64().PrimaryKey().Identity()
            .WithColumn("name").AsString(255).NotNullable()
            .WithColumn("company_name").AsString(255).Nullable()
            .WithColumn("code").AsInt32().Nullable().Unique()
            .WithColumn("email").AsString(255).Nullable()
            .WithColumn("city").AsString(255).Nullable()
            .WithColumn("phone").AsString(255).NotNullable()
            .WithColumn("address").AsString(255).Nullable()
            .WithColumn("nit_ci").AsString(255).NotNullable()
            .WithAuditFields();

        Create.Table("providers")
            .WithColumn("id").AsInt64().PrimaryKey().Identity()
            .WithColumn("name").AsString(255).NotNullable()
            .WithColumn("code").AsInt32().NotNullable()
            .WithColumn("email").AsString(192).NotNullable()
            .WithColumn("phone").AsString(192).Nullable()
            .WithColumn("country").AsString(255).Nullable()
            .WithColumn("city").AsString(255).Nullable()
            .WithColumn("address").AsString(255).Nullable()
            .WithAuditFields();
    }

    public override void Down()
    {
        Delete.Table("providers");
        Delete.Table("clients");
    }
}
