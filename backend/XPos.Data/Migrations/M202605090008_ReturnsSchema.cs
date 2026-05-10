using FluentMigrator;

namespace XPos.Data.Migrations;

[Migration(202605090008)]
public class M202605090008_ReturnsSchema : Migration
{
    public override void Up()
    {
        Create.Table("sale_returns")
            .WithColumn("id").AsInt64().PrimaryKey().Identity()
            .WithColumn("user_id").AsInt64().ForeignKey("users", "id")
            .WithColumn("date").AsDate().NotNullable()
            .WithColumn("ref").AsString(192).NotNullable()
            .WithColumn("sale_id").AsInt64().Nullable().ForeignKey("sales", "id")
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
            .WithColumn("notes").AsCustom("TEXT").Nullable()
            .WithAuditFields();

        Create.Table("sale_return_details")
            .WithColumn("id").AsInt64().PrimaryKey().Identity()
            .WithColumn("sale_return_id").AsInt64().ForeignKey("sale_returns", "id")
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
            .WithColumn("updated_at").AsDateTime().Nullable();

        Create.Table("purchase_returns")
            .WithColumn("id").AsInt64().PrimaryKey().Identity()
            .WithColumn("user_id").AsInt64().ForeignKey("users", "id")
            .WithColumn("date").AsDate().NotNullable()
            .WithColumn("ref").AsString(192).NotNullable()
            .WithColumn("purchase_id").AsInt64().Nullable().ForeignKey("purchases", "id")
            .WithColumn("provider_id").AsInt64().ForeignKey("providers", "id")
            .WithColumn("warehouse_id").AsInt64().ForeignKey("warehouses", "id")
            .WithColumn("tax_net").AsFloat().Nullable().WithDefaultValue(0)
            .WithColumn("tax_rate").AsFloat().Nullable().WithDefaultValue(0)
            .WithColumn("discount").AsFloat().Nullable().WithDefaultValue(0)
            .WithColumn("shipping").AsFloat().Nullable().WithDefaultValue(0)
            .WithColumn("grand_total").AsFloat().NotNullable()
            .WithColumn("paid_amount").AsFloat().WithDefaultValue(0)
            .WithColumn("payment_status").AsString(192).NotNullable()
            .WithColumn("status").AsString(255).NotNullable()
            .WithColumn("notes").AsCustom("TEXT").Nullable()
            .WithAuditFields();

        Create.Table("purchase_return_details")
            .WithColumn("id").AsInt64().PrimaryKey().Identity()
            .WithColumn("purchase_return_id").AsInt64().ForeignKey("purchase_returns", "id")
            .WithColumn("product_id").AsInt64().ForeignKey("products", "id")
            .WithColumn("product_variant_id").AsInt64().Nullable().ForeignKey("product_variants", "id")
            .WithColumn("purchase_unit_id").AsInt64().Nullable().ForeignKey("units", "id")
            .WithColumn("cost").AsFloat().NotNullable()
            .WithColumn("tax_net").AsFloat().Nullable().WithDefaultValue(0)
            .WithColumn("tax_method").AsString(192).Nullable().WithDefaultValue("1")
            .WithColumn("discount").AsFloat().Nullable().WithDefaultValue(0)
            .WithColumn("discount_method").AsString(192).Nullable().WithDefaultValue("1")
            .WithColumn("total").AsFloat().NotNullable()
            .WithColumn("quantity").AsFloat().NotNullable()
            .WithColumn("created_at").AsDateTime().Nullable()
            .WithColumn("updated_at").AsDateTime().Nullable();
    }

    public override void Down()
    {
        Delete.Table("purchase_return_details");
        Delete.Table("purchase_returns");
        Delete.Table("sale_return_details");
        Delete.Table("sale_returns");
    }
}
