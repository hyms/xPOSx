using FluentMigrator;

namespace XPos.Data.Migrations;

[Migration(202605090006)]
public class M202605090006_ExpenseSchema : Migration
{
    public override void Up()
    {
        Create.Table("expense_categories")
            .WithColumn("id").AsInt64().PrimaryKey().Identity()
            .WithColumn("user_id").AsInt64().ForeignKey("users", "id")
            .WithColumn("name").AsString(255).NotNullable()
            .WithColumn("description").AsCustom("TEXT").Nullable()
            .WithAuditFields();

        Create.Table("expenses")
            .WithColumn("id").AsInt64().PrimaryKey().Identity()
            .WithColumn("date").AsDate().NotNullable()
            .WithColumn("ref").AsString(192).NotNullable()
            .WithColumn("user_id").AsInt64().ForeignKey("users", "id")
            .WithColumn("expense_category_id").AsInt64().ForeignKey("expense_categories", "id")
            .WithColumn("warehouse_id").AsInt64().ForeignKey("warehouses", "id")
            .WithColumn("details").AsString(255).NotNullable()
            .WithColumn("amount").AsFloat().NotNullable()
            .WithAuditFields();
    }

    public override void Down()
    {
        Delete.Table("expenses");
        Delete.Table("expense_categories");
    }
}
