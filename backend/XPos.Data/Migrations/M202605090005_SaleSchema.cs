using FluentMigrator;

namespace XPos.Data.Migrations;

[Migration(202605090005)]
public class M202605090005_SaleSchema : Migration
{
    public override void Up()
    {
        Create.Table("sales")
            .WithColumn("id").AsInt64().PrimaryKey().Identity()
            .WithColumn("user_id").AsInt64().ForeignKey("users", "id")
            .WithColumn("date").AsDate().NotNullable()
            .WithColumn("ref").AsString(192).NotNullable()
            .WithColumn("is_pos").AsBoolean().Nullable().WithDefaultValue(false)
            .WithColumn("client_id").AsInt64().ForeignKey("clients", "id")
            .WithColumn("warehouse_id").AsInt64().ForeignKey("warehouses", "id")
            .WithColumn("tax_net").AsFloat().Nullable().WithDefaultValue(0)
            .WithColumn("tax_rate").AsFloat().Nullable().WithDefaultValue(0)
            .WithColumn("discount").AsFloat().Nullable().WithDefaultValue(0)
            .WithColumn("shipping").AsFloat().Nullable().WithDefaultValue(0)
            .WithColumn("grand_total").AsFloat().WithDefaultValue(0)
            .WithColumn("paid_amount").AsFloat().WithDefaultValue(0)
            .WithColumn("payment_status").AsString(255).NotNullable()
            .WithColumn("status").AsString(255).NotNullable()
            .WithColumn("shipping_status").AsString(255).Nullable()
            .WithColumn("notes").AsCustom("TEXT").Nullable()
            .WithAuditFields();

        Create.Table("sale_details")
            .WithColumn("id").AsInt64().PrimaryKey().Identity()
            .WithColumn("date").AsDate().NotNullable()
            .WithColumn("sale_id").AsInt64().ForeignKey("sales", "id")
            .WithColumn("product_id").AsInt64().ForeignKey("products", "id")
            .WithColumn("product_variant_id").AsInt64().Nullable().ForeignKey("product_variants", "id")
            .WithColumn("sale_unit_id").AsInt64().Nullable().ForeignKey("units", "id")
            .WithColumn("price").AsFloat().NotNullable()
            .WithColumn("tax_net").AsFloat().Nullable().WithDefaultValue(0)
            .WithColumn("tax_method").AsString(255).Nullable().WithDefaultValue("1")
            .WithColumn("discount").AsFloat().Nullable().WithDefaultValue(0)
            .WithColumn("discount_method").AsString(255).Nullable().WithDefaultValue("1")
            .WithColumn("total").AsFloat().NotNullable()
            .WithColumn("quantity").AsFloat().NotNullable()
            .WithColumn("created_at").AsDateTime().Nullable()
            .WithColumn("updated_at").AsDateTime().Nullable()
            .WithColumn("created_by").AsInt64().Nullable().ForeignKey("users", "id")
            .WithColumn("updated_by").AsInt64().Nullable().ForeignKey("users", "id");

        Create.Table("payment_sales")
            .WithColumn("id").AsInt64().PrimaryKey().Identity()
            .WithColumn("user_id").AsInt64().ForeignKey("users", "id")
            .WithColumn("date").AsDate().NotNullable()
            .WithColumn("ref").AsString(192).NotNullable()
            .WithColumn("sale_id").AsInt64().ForeignKey("sales", "id")
            .WithColumn("amount").AsFloat().NotNullable()
            .WithColumn("change").AsFloat().WithDefaultValue(0)
            .WithColumn("reglement").AsString(255).NotNullable()
            .WithColumn("notes").AsCustom("TEXT").Nullable()
            .WithAuditFields();
    }

    public override void Down()
    {
        Delete.Table("payment_sales");
        Delete.Table("sale_details");
        Delete.Table("sales");
    }
}
