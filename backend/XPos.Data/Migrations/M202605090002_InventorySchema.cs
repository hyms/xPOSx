using FluentMigrator;

namespace XPos.Data.Migrations;

[Migration(202605090002)]
public class M202605090002_InventorySchema : Migration
{
    public override void Up()
    {
        Create.Table("categories")
            .WithColumn("id").AsInt64().PrimaryKey().Identity()
            .WithColumn("code").AsString(255).NotNullable()
            .WithColumn("name").AsString(255).NotNullable()
            .WithAuditFields();

        Create.Table("units")
            .WithColumn("id").AsInt64().PrimaryKey().Identity()
            .WithColumn("name").AsString(255).NotNullable()
            .WithColumn("short_name").AsString(255).NotNullable()
            .WithColumn("base_unit").AsInt64().Nullable().ForeignKey("units", "id")
            .WithColumn("operator").AsString(1).Nullable().WithDefaultValue("*")
            .WithColumn("operator_value").AsFloat().Nullable().WithDefaultValue(1)
            .WithAuditFields();

        Create.Table("products")
            .WithColumn("id").AsInt64().PrimaryKey().Identity()
            .WithColumn("code").AsString(255).NotNullable()
            .WithColumn("name").AsString(255).NotNullable()
            .WithColumn("cost").AsFloat().NotNullable()
            .WithColumn("price").AsFloat().NotNullable()
            .WithColumn("category_id").AsInt64().ForeignKey("categories", "id")
            .WithColumn("unit_id").AsInt64().Nullable().ForeignKey("units", "id")
            .WithColumn("unit_sale_id").AsInt64().Nullable().ForeignKey("units", "id")
            .WithColumn("unit_purchase_id").AsInt64().Nullable().ForeignKey("units", "id")
            .WithColumn("tax_net").AsFloat().Nullable().WithDefaultValue(0)
            .WithColumn("tax_method").AsString(255).Nullable().WithDefaultValue("1")
            .WithColumn("note").AsCustom("TEXT").Nullable()
            .WithColumn("stock_alert").AsFloat().Nullable().WithDefaultValue(0)
            .WithColumn("is_variant").AsBoolean().WithDefaultValue(false)
            .WithColumn("not_selling").AsBoolean().WithDefaultValue(false)
            .WithColumn("is_active").AsBoolean().WithDefaultValue(true)
            .WithAuditFields();

        Create.Table("product_variants")
            .WithColumn("id").AsInt64().PrimaryKey().Identity()
            .WithColumn("product_id").AsInt64().ForeignKey("products", "id")
            .WithColumn("name").AsString(255).Nullable()
            .WithColumn("qty").AsDecimal(18, 2).Nullable().WithDefaultValue(0)
            .WithColumn("price").AsFloat().Nullable()
            .WithAuditFields();

        Create.Table("product_warehouse")
            .WithColumn("id").AsInt64().PrimaryKey().Identity()
            .WithColumn("product_id").AsInt64().ForeignKey("products", "id")
            .WithColumn("warehouse_id").AsInt64().ForeignKey("warehouses", "id")
            .WithColumn("product_variant_id").AsInt64().Nullable().ForeignKey("product_variants", "id")
            .WithColumn("qty").AsFloat().WithDefaultValue(0)
            .WithColumn("price").AsFloat().WithDefaultValue(0)
            .WithAuditFields();
    }

    public override void Down()
    {
        Delete.Table("product_warehouse");
        Delete.Table("product_variants");
        Delete.Table("products");
        Delete.Table("units");
        Delete.Table("categories");
    }
}
