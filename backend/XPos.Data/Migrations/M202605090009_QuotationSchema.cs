using FluentMigrator;

namespace XPos.Data.Migrations;

[Migration(202605090009)]
public class M202605090009_QuotationSchema : Migration
{
    public override void Up()
    {
        Create.Table("quotations")
            .WithColumn("id").AsInt64().PrimaryKey().Identity()
            .WithColumn("user_id").AsInt64().ForeignKey("users", "id")
            .WithColumn("date").AsDate().NotNullable()
            .WithColumn("ref").AsString(192).NotNullable()
            .WithColumn("client_id").AsInt64().ForeignKey("clients", "id")
            .WithColumn("warehouse_id").AsInt64().ForeignKey("warehouses", "id")
            .WithColumn("tax_net").AsFloat().Nullable().WithDefaultValue(0)
            .WithColumn("tax_rate").AsFloat().Nullable().WithDefaultValue(0)
            .WithColumn("discount").AsFloat().Nullable().WithDefaultValue(0)
            .WithColumn("shipping").AsFloat().Nullable().WithDefaultValue(0)
            .WithColumn("grand_total").AsFloat().WithDefaultValue(0)
            .WithColumn("status").AsString(255).NotNullable().WithDefaultValue("pending")
            .WithColumn("notes").AsCustom("TEXT").Nullable()
            .WithAuditFields();

        Create.Table("quotation_details")
            .WithColumn("id").AsInt64().PrimaryKey().Identity()
            .WithColumn("quotation_id").AsInt64().ForeignKey("quotations", "id")
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
    }

    public override void Down()
    {
        Delete.Table("quotation_details");
        Delete.Table("quotations");
    }
}
