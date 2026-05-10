using FluentMigrator;

namespace XPos.Data.Migrations;

[Migration(202605090004)]
public class M202605090004_PurchaseSchema : Migration
{
    public override void Up()
    {
        Create.Table("purchases")
            .WithColumn("id").AsInt64().PrimaryKey().Identity()
            .WithColumn("user_id").AsInt64().ForeignKey("users", "id")
            .WithColumn("ref").AsString(192).NotNullable()
            .WithColumn("date").AsDate().NotNullable()
            .WithColumn("provider_id").AsInt64().ForeignKey("providers", "id")
            .WithColumn("warehouse_id").AsInt64().ForeignKey("warehouses", "id")
            .WithColumn("tax_rate").AsFloat().Nullable().WithDefaultValue(0)
            .WithColumn("tax_net").AsFloat().Nullable().WithDefaultValue(0)
            .WithColumn("discount").AsFloat().Nullable().WithDefaultValue(0)
            .WithColumn("shipping").AsFloat().Nullable().WithDefaultValue(0)
            .WithColumn("grand_total").AsFloat().NotNullable()
            .WithColumn("paid_amount").AsFloat().WithDefaultValue(0)
            .WithColumn("status").AsString(255).NotNullable()
            .WithColumn("payment_status").AsString(192).NotNullable()
            .WithColumn("notes").AsCustom("TEXT").Nullable()
            .WithAuditFields();

        Create.Table("purchase_details")
            .WithColumn("id").AsInt64().PrimaryKey().Identity()
            .WithColumn("cost").AsFloat().NotNullable()
            .WithColumn("purchase_unit_id").AsInt64().Nullable().ForeignKey("units", "id")
            .WithColumn("tax_net").AsFloat().Nullable().WithDefaultValue(0)
            .WithColumn("tax_method").AsString(192).Nullable().WithDefaultValue("1")
            .WithColumn("discount").AsFloat().Nullable().WithDefaultValue(0)
            .WithColumn("discount_method").AsString(192).Nullable().WithDefaultValue("1")
            .WithColumn("purchase_id").AsInt64().ForeignKey("purchases", "id")
            .WithColumn("product_id").AsInt64().ForeignKey("products", "id")
            .WithColumn("product_variant_id").AsInt64().Nullable().ForeignKey("product_variants", "id")
            .WithColumn("total").AsFloat().NotNullable()
            .WithColumn("quantity").AsFloat().NotNullable()
            .WithColumn("created_at").AsDateTime().Nullable()
            .WithColumn("updated_at").AsDateTime().Nullable()
            .WithColumn("created_by").AsInt64().Nullable().ForeignKey("users", "id")
            .WithColumn("updated_by").AsInt64().Nullable().ForeignKey("users", "id");

        Create.Table("payment_purchases")
            .WithColumn("id").AsInt64().PrimaryKey().Identity()
            .WithColumn("user_id").AsInt64().ForeignKey("users", "id")
            .WithColumn("date").AsDate().NotNullable()
            .WithColumn("ref").AsString(192).NotNullable()
            .WithColumn("purchase_id").AsInt64().ForeignKey("purchases", "id")
            .WithColumn("amount").AsFloat().NotNullable()
            .WithColumn("change").AsFloat().WithDefaultValue(0)
            .WithColumn("reglement").AsString(192).NotNullable()
            .WithColumn("notes").AsCustom("TEXT").Nullable()
            .WithAuditFields();
    }

    public override void Down()
    {
        Delete.Table("payment_purchases");
        Delete.Table("purchase_details");
        Delete.Table("purchases");
    }
}
