using FluentMigrator;

namespace XPos.Data.Migrations;

[Migration(202605090007)]
public class M202605090007_InventoryOpsSchema : Migration
{
    public override void Up()
    {
        Create.Table("adjustments")
            .WithColumn("id").AsInt64().PrimaryKey().Identity()
            .WithColumn("user_id").AsInt64().ForeignKey("users", "id")
            .WithColumn("date").AsDate().NotNullable()
            .WithColumn("ref").AsString(192).NotNullable()
            .WithColumn("warehouse_id").AsInt64().ForeignKey("warehouses", "id")
            .WithColumn("items").AsFloat().Nullable().WithDefaultValue(0)
            .WithColumn("notes").AsCustom("TEXT").Nullable()
            .WithAuditFields();

        Create.Table("adjustment_details")
            .WithColumn("id").AsInt64().PrimaryKey().Identity()
            .WithColumn("product_id").AsInt64().ForeignKey("products", "id")
            .WithColumn("product_variant_id").AsInt64().Nullable().ForeignKey("product_variants", "id")
            .WithColumn("adjustment_id").AsInt64().ForeignKey("adjustments", "id")
            .WithColumn("quantity").AsFloat().NotNullable()
            .WithColumn("type").AsString(255).NotNullable()
            .WithColumn("created_at").AsDateTime().Nullable()
            .WithColumn("updated_at").AsDateTime().Nullable()
            .WithColumn("created_by").AsInt64().Nullable().ForeignKey("users", "id")
            .WithColumn("updated_by").AsInt64().Nullable().ForeignKey("users", "id");

        Create.Table("transfers")
            .WithColumn("id").AsInt64().PrimaryKey().Identity()
            .WithColumn("user_id").AsInt64().ForeignKey("users", "id")
            .WithColumn("ref").AsString(192).NotNullable()
            .WithColumn("date").AsDate().NotNullable()
            .WithColumn("from_warehouse_id").AsInt64().ForeignKey("warehouses", "id")
            .WithColumn("to_warehouse_id").AsInt64().ForeignKey("warehouses", "id")
            .WithColumn("items").AsFloat().NotNullable()
            .WithColumn("tax_rate").AsFloat().Nullable().WithDefaultValue(0)
            .WithColumn("tax_net").AsFloat().Nullable().WithDefaultValue(0)
            .WithColumn("discount").AsFloat().Nullable().WithDefaultValue(0)
            .WithColumn("shipping").AsFloat().Nullable().WithDefaultValue(0)
            .WithColumn("grand_total").AsFloat().WithDefaultValue(0)
            .WithColumn("status").AsString(192).NotNullable()
            .WithColumn("notes").AsCustom("TEXT").Nullable()
            .WithAuditFields();

        Create.Table("transfer_details")
            .WithColumn("id").AsInt64().PrimaryKey().Identity()
            .WithColumn("transfer_id").AsInt64().ForeignKey("transfers", "id")
            .WithColumn("product_id").AsInt64().ForeignKey("products", "id")
            .WithColumn("product_variant_id").AsInt64().Nullable().ForeignKey("product_variants", "id")
            .WithColumn("purchase_unit_id").AsInt64().Nullable().ForeignKey("units", "id")
            .WithColumn("cost").AsFloat().NotNullable()
            .WithColumn("tax_net").AsFloat().Nullable()
            .WithColumn("tax_method").AsString(192).Nullable().WithDefaultValue("1")
            .WithColumn("discount").AsFloat().Nullable()
            .WithColumn("discount_method").AsString(192).Nullable().WithDefaultValue("1")
            .WithColumn("quantity").AsFloat().NotNullable()
            .WithColumn("total").AsFloat().NotNullable()
            .WithAuditFields();
    }

    public override void Down()
    {
        Delete.Table("transfer_details");
        Delete.Table("transfers");
        Delete.Table("adjustment_details");
        Delete.Table("adjustments");
    }
}
