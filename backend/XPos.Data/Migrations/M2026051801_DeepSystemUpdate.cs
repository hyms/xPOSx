using FluentMigrator;
using System.Data;

namespace XPos.Data.Migrations;

[Migration(2026051801)]
public class M2026051801_DeepSystemUpdate : Migration
{
    public override void Up()
    {
        // 1. Update users table: role_id FK and remove role column
        Alter.Table("users")
            .AddColumn("role_id").AsInt64().Nullable()
                .ForeignKey("roles", "id").OnDelete(Rule.SetNull)
            .AddColumn("default_warehouse_id").AsInt64().Nullable().ForeignKey("warehouses", "id");

        // Migrate data from 'role' to 'role_id' if possible
        Execute.Sql("UPDATE users SET role_id = role WHERE role IN (SELECT id FROM roles)");
        
        Rename.Column("role").OnTable("users").To("role_old");

        // 2. Unify Currencies into Settings (YAGNI/KISS)
        Alter.Table("settings")
            .AddColumn("currency_code").AsString(10).Nullable()
            .AddColumn("currency_symbol").AsString(10).Nullable()
            .AddColumn("currency_name").AsString(50).Nullable();

        // Migrate data from currencies if it exists (assuming BOB as default for SIAT)
        Execute.Sql(@"
            UPDATE settings 
            SET currency_code = COALESCE((SELECT code FROM currencies LIMIT 1), 'BOB'),
                currency_symbol = COALESCE((SELECT symbol FROM currencies LIMIT 1), 'Bs'),
                currency_name = COALESCE((SELECT name FROM currencies LIMIT 1), 'Boliviano')
            WHERE id = (SELECT id FROM settings ORDER BY id LIMIT 1)");

        Rename.Table("currencies").To("currencies_old");
        Rename.Table("CurrencySettings").To("CurrencySettings_old");

        // 3. Consolidate settings: mail, sms, and payment gateway
        Alter.Table("settings")
            // Mail settings
            .AddColumn("mail_host").AsString(255).Nullable()
            .AddColumn("mail_port").AsInt32().Nullable()
            .AddColumn("mail_username").AsString(255).Nullable()
            .AddColumn("mail_password").AsString(255).Nullable()
            .AddColumn("mail_encryption").AsString(50).Nullable()
            .AddColumn("mail_from_address").AsString(255).Nullable()
            .AddColumn("mail_from_name").AsString(255).Nullable()
            // SMS settings
            .AddColumn("sms_gateway_name").AsString(100).Nullable()
            .AddColumn("sms_api_key").AsString(255).Nullable()
            .AddColumn("sms_api_secret").AsString(255).Nullable()
            .AddColumn("sms_sender_id").AsString(100).Nullable()
            .AddColumn("sms_is_active").AsBoolean().WithDefaultValue(false)
            .AddColumn("sms_description").AsString(500).Nullable()
            // Payment Gateway settings
            .AddColumn("payment_gateway_name").AsString(100).Nullable()
            .AddColumn("payment_api_key").AsString(255).Nullable()
            .AddColumn("payment_api_secret").AsString(255).Nullable()
            .AddColumn("payment_is_active").AsBoolean().WithDefaultValue(false)
            .AddColumn("payment_description").AsString(500).Nullable();

        // 4. SIAT Bolivia fields in settings
        Alter.Table("settings")
            .AddColumn("siat_token").AsString(2000).Nullable()
            .AddColumn("siat_certificate").AsCustom("TEXT").Nullable() // Base64 encoded
            .AddColumn("siat_environment").AsInt32().Nullable()
            .AddColumn("siat_modality").AsInt32().Nullable()
            .AddColumn("siat_emission_type").AsInt32().Nullable();

        // Delete redundant settings tables
        Rename.Table("mail_settings").To("mail_settings_old");
        Rename.Table("sms_settings").To("sms_settings_old");
        Rename.Table("payment_gateway_settings").To("payment_gateway_settings_old");

        // 5. Optimize user_warehouse: Composite PK
        Create.PrimaryKey("PK_user_warehouse")
            .OnTable("user_warehouse")
            .Columns("user_id", "warehouse_id");

        // 5b. Indexes for warehouse_id in business tables
        Create.Index("IX_sales_warehouse_id").OnTable("sales").OnColumn("warehouse_id");
        Create.Index("IX_purchases_warehouse_id").OnTable("purchases").OnColumn("warehouse_id");
        Create.Index("IX_expenses_warehouse_id").OnTable("expenses").OnColumn("warehouse_id");
        Create.Index("IX_quotations_warehouse_id").OnTable("quotations").OnColumn("warehouse_id");
        Create.Index("IX_adjustments_warehouse_id").OnTable("adjustments").OnColumn("warehouse_id");
        Create.Index("IX_transfers_from_warehouse_id").OnTable("transfers").OnColumn("from_warehouse_id");
        Create.Index("IX_transfers_to_warehouse_id").OnTable("transfers").OnColumn("to_warehouse_id");
        Create.Index("IX_sale_returns_warehouse_id").OnTable("sale_returns").OnColumn("warehouse_id");
        Create.Index("IX_purchase_returns_warehouse_id").OnTable("purchase_returns").OnColumn("warehouse_id");
        Create.Index("IX_product_warehouse_warehouse_id").OnTable("product_warehouse").OnColumn("warehouse_id");

        // 6. Precision update: real to numeric(18,2)
        // Products & Variants
        Alter.Table("products").AlterColumn("cost").AsDecimal(18, 2);
        Alter.Table("products").AlterColumn("price").AsDecimal(18, 2);
        Alter.Table("products").AlterColumn("tax_net").AsDecimal(18, 2);
        Alter.Table("products").AlterColumn("stock_alert").AsDecimal(18, 2);
        Alter.Table("product_variants").AlterColumn("price").AsDecimal(18, 2);
        Alter.Table("product_warehouse").AlterColumn("qty").AsDecimal(18, 2);
        Alter.Table("product_warehouse").AlterColumn("price").AsDecimal(18, 2);
        
        // Units
        Alter.Table("units").AlterColumn("operator_value").AsDecimal(18, 2);
        
        // Expenses
        Alter.Table("expenses").AlterColumn("amount").AsDecimal(18, 2);
        
        // Quotations
        Alter.Table("quotations").AlterColumn("tax_net").AsDecimal(18, 2);
        Alter.Table("quotations").AlterColumn("tax_rate").AsDecimal(18, 2);
        Alter.Table("quotations").AlterColumn("discount").AsDecimal(18, 2);
        Alter.Table("quotations").AlterColumn("shipping").AsDecimal(18, 2);
        Alter.Table("quotations").AlterColumn("grand_total").AsDecimal(18, 2);
        Alter.Table("quotation_details").AlterColumn("price").AsDecimal(18, 2);
        Alter.Table("quotation_details").AlterColumn("tax_net").AsDecimal(18, 2);
        Alter.Table("quotation_details").AlterColumn("discount").AsDecimal(18, 2);
        Alter.Table("quotation_details").AlterColumn("total").AsDecimal(18, 2);
        Alter.Table("quotation_details").AlterColumn("quantity").AsDecimal(18, 2);
        
        // Purchases
        Alter.Table("purchases").AlterColumn("tax_rate").AsDecimal(18, 2);
        Alter.Table("purchases").AlterColumn("tax_net").AsDecimal(18, 2);
        Alter.Table("purchases").AlterColumn("discount").AsDecimal(18, 2);
        Alter.Table("purchases").AlterColumn("shipping").AsDecimal(18, 2);
        Alter.Table("purchases").AlterColumn("grand_total").AsDecimal(18, 2);
        Alter.Table("purchases").AlterColumn("paid_amount").AsDecimal(18, 2);
        Alter.Table("purchase_details").AlterColumn("cost").AsDecimal(18, 2);
        Alter.Table("purchase_details").AlterColumn("tax_net").AsDecimal(18, 2);
        Alter.Table("purchase_details").AlterColumn("discount").AsDecimal(18, 2);
        Alter.Table("purchase_details").AlterColumn("quantity").AsDecimal(18, 2);
        Alter.Table("purchase_details").AlterColumn("total").AsDecimal(18, 2);
        if (Schema.Table("payment_purchases").Exists())
        {
            Alter.Table("payment_purchases").AlterColumn("amount").AsDecimal(18, 2);
            Alter.Table("payment_purchases").AlterColumn("change").AsDecimal(18, 2);
        }
        
        // Sales
        Alter.Table("sales").AlterColumn("tax_net").AsDecimal(18, 2);
        Alter.Table("sales").AlterColumn("tax_rate").AsDecimal(18, 2);
        Alter.Table("sales").AlterColumn("discount").AsDecimal(18, 2);
        Alter.Table("sales").AlterColumn("shipping").AsDecimal(18, 2);
        Alter.Table("sales").AlterColumn("grand_total").AsDecimal(18, 2);
        Alter.Table("sales").AlterColumn("paid_amount").AsDecimal(18, 2);
        Alter.Table("sale_details").AlterColumn("price").AsDecimal(18, 2);
        Alter.Table("sale_details").AlterColumn("tax_net").AsDecimal(18, 2);
        Alter.Table("sale_details").AlterColumn("discount").AsDecimal(18, 2);
        Alter.Table("sale_details").AlterColumn("total").AsDecimal(18, 2);
        Alter.Table("sale_details").AlterColumn("quantity").AsDecimal(18, 2);
        if (Schema.Table("sale_payments").Exists())
        {
            Alter.Table("sale_payments").AlterColumn("amount").AsDecimal(18, 2);
            Alter.Table("sale_payments").AlterColumn("change").AsDecimal(18, 2);
        }
        
        // Returns
        Alter.Table("sale_returns").AlterColumn("tax_net").AsDecimal(18, 2);
        Alter.Table("sale_returns").AlterColumn("tax_rate").AsDecimal(18, 2);
        Alter.Table("sale_returns").AlterColumn("discount").AsDecimal(18, 2);
        Alter.Table("sale_returns").AlterColumn("shipping").AsDecimal(18, 2);
        Alter.Table("sale_returns").AlterColumn("grand_total").AsDecimal(18, 2);
        Alter.Table("sale_returns").AlterColumn("paid_amount").AsDecimal(18, 2);
        Alter.Table("sale_return_details").AlterColumn("price").AsDecimal(18, 2);
        Alter.Table("sale_return_details").AlterColumn("tax_net").AsDecimal(18, 2);
        Alter.Table("sale_return_details").AlterColumn("discount").AsDecimal(18, 2);
        Alter.Table("sale_return_details").AlterColumn("total").AsDecimal(18, 2);
        Alter.Table("sale_return_details").AlterColumn("quantity").AsDecimal(18, 2);
        
        Alter.Table("purchase_returns").AlterColumn("tax_net").AsDecimal(18, 2);
        Alter.Table("purchase_returns").AlterColumn("tax_rate").AsDecimal(18, 2);
        Alter.Table("purchase_returns").AlterColumn("discount").AsDecimal(18, 2);
        Alter.Table("purchase_returns").AlterColumn("shipping").AsDecimal(18, 2);
        Alter.Table("purchase_returns").AlterColumn("grand_total").AsDecimal(18, 2);
        Alter.Table("purchase_returns").AlterColumn("paid_amount").AsDecimal(18, 2);
        Alter.Table("purchase_return_details").AlterColumn("cost").AsDecimal(18, 2);
        Alter.Table("purchase_return_details").AlterColumn("tax_net").AsDecimal(18, 2);
        Alter.Table("purchase_return_details").AlterColumn("discount").AsDecimal(18, 2);
        Alter.Table("purchase_return_details").AlterColumn("total").AsDecimal(18, 2);
        Alter.Table("purchase_return_details").AlterColumn("quantity").AsDecimal(18, 2);

        // Adjustments & Transfers
        Alter.Table("adjustments").AlterColumn("items").AsDecimal(18, 2);
        Alter.Table("adjustment_details").AlterColumn("quantity").AsDecimal(18, 2);
        Alter.Table("transfers").AlterColumn("items").AsDecimal(18, 2);
        Alter.Table("transfer_details").AlterColumn("quantity").AsDecimal(18, 2);
    }

    public override void Down()
    {
        // Reverse precision (back to real/float)
        Alter.Table("products").AlterColumn("cost").AsFloat();
        Alter.Table("products").AlterColumn("price").AsFloat();
        Alter.Table("products").AlterColumn("tax_net").AsFloat();
        Alter.Table("products").AlterColumn("stock_alert").AsFloat();
        Alter.Table("product_variants").AlterColumn("price").AsFloat();
        Alter.Table("product_warehouse").AlterColumn("qty").AsFloat();
        Alter.Table("product_warehouse").AlterColumn("price").AsFloat();
        Alter.Table("units").AlterColumn("operator_value").AsFloat();
        Alter.Table("expenses").AlterColumn("amount").AsFloat();
        
        Alter.Table("quotations").AlterColumn("tax_net").AsFloat();
        Alter.Table("quotations").AlterColumn("tax_rate").AsFloat();
        Alter.Table("quotations").AlterColumn("discount").AsFloat();
        Alter.Table("quotations").AlterColumn("shipping").AsFloat();
        Alter.Table("quotations").AlterColumn("grand_total").AsFloat();
        Alter.Table("quotation_details").AlterColumn("price").AsFloat();
        Alter.Table("quotation_details").AlterColumn("tax_net").AsFloat();
        Alter.Table("quotation_details").AlterColumn("discount").AsFloat();
        Alter.Table("quotation_details").AlterColumn("total").AsFloat();
        Alter.Table("quotation_details").AlterColumn("quantity").AsFloat();
        
        Alter.Table("purchases").AlterColumn("tax_rate").AsFloat();
        Alter.Table("purchases").AlterColumn("tax_net").AsFloat();
        Alter.Table("purchases").AlterColumn("discount").AsFloat();
        Alter.Table("purchases").AlterColumn("shipping").AsFloat();
        Alter.Table("purchases").AlterColumn("grand_total").AsFloat();
        Alter.Table("purchases").AlterColumn("paid_amount").AsFloat();
        Alter.Table("purchase_details").AlterColumn("cost").AsFloat();
        Alter.Table("purchase_details").AlterColumn("tax_net").AsFloat();
        Alter.Table("purchase_details").AlterColumn("discount").AsFloat();
        Alter.Table("purchase_details").AlterColumn("quantity").AsFloat();
        Alter.Table("purchase_details").AlterColumn("total").AsFloat();
        if (Schema.Table("payment_purchases").Exists())
        {
            Alter.Table("payment_purchases").AlterColumn("amount").AsFloat();
            Alter.Table("payment_purchases").AlterColumn("change").AsFloat();
        }
        
        Alter.Table("sales").AlterColumn("tax_net").AsFloat();
        Alter.Table("sales").AlterColumn("tax_rate").AsFloat();
        Alter.Table("sales").AlterColumn("discount").AsFloat();
        Alter.Table("sales").AlterColumn("shipping").AsFloat();
        Alter.Table("sales").AlterColumn("grand_total").AsFloat();
        Alter.Table("sales").AlterColumn("paid_amount").AsFloat();
        Alter.Table("sale_details").AlterColumn("price").AsFloat();
        Alter.Table("sale_details").AlterColumn("tax_net").AsFloat();
        Alter.Table("sale_details").AlterColumn("discount").AsFloat();
        Alter.Table("sale_details").AlterColumn("total").AsFloat();
        Alter.Table("sale_details").AlterColumn("quantity").AsFloat();
        if (Schema.Table("sale_payments").Exists())
        {
            Alter.Table("sale_payments").AlterColumn("amount").AsFloat();
            Alter.Table("sale_payments").AlterColumn("change").AsFloat();
        }
        
        Alter.Table("sale_returns").AlterColumn("tax_net").AsFloat();
        Alter.Table("sale_returns").AlterColumn("tax_rate").AsFloat();
        Alter.Table("sale_returns").AlterColumn("discount").AsFloat();
        Alter.Table("sale_returns").AlterColumn("shipping").AsFloat();
        Alter.Table("sale_returns").AlterColumn("grand_total").AsFloat();
        Alter.Table("sale_returns").AlterColumn("paid_amount").AsFloat();
        Alter.Table("sale_return_details").AlterColumn("price").AsFloat();
        Alter.Table("sale_return_details").AlterColumn("tax_net").AsFloat();
        Alter.Table("sale_return_details").AlterColumn("discount").AsFloat();
        Alter.Table("sale_return_details").AlterColumn("total").AsFloat();
        Alter.Table("sale_return_details").AlterColumn("quantity").AsFloat();
        
        Alter.Table("purchase_returns").AlterColumn("tax_net").AsFloat();
        Alter.Table("purchase_returns").AlterColumn("tax_rate").AsFloat();
        Alter.Table("purchase_returns").AlterColumn("discount").AsFloat();
        Alter.Table("purchase_returns").AlterColumn("shipping").AsFloat();
        Alter.Table("purchase_returns").AlterColumn("grand_total").AsFloat();
        Alter.Table("purchase_returns").AlterColumn("paid_amount").AsFloat();
        Alter.Table("purchase_return_details").AlterColumn("cost").AsFloat();
        Alter.Table("purchase_return_details").AlterColumn("tax_net").AsFloat();
        Alter.Table("purchase_return_details").AlterColumn("discount").AsFloat();
        Alter.Table("purchase_return_details").AlterColumn("total").AsFloat();
        Alter.Table("purchase_return_details").AlterColumn("quantity").AsFloat();

        Alter.Table("adjustments").AlterColumn("items").AsFloat();
        Alter.Table("adjustment_details").AlterColumn("quantity").AsFloat();
        Alter.Table("transfers").AlterColumn("items").AsFloat();
        Alter.Table("transfer_details").AlterColumn("quantity").AsFloat();

        // Remove indexes
        Delete.Index("IX_product_warehouse_warehouse_id").OnTable("product_warehouse");
        Delete.Index("IX_purchase_returns_warehouse_id").OnTable("purchase_returns");
        Delete.Index("IX_sale_returns_warehouse_id").OnTable("sale_returns");
        Delete.Index("IX_transfers_to_warehouse_id").OnTable("transfers");
        Delete.Index("IX_transfers_from_warehouse_id").OnTable("transfers");
        Delete.Index("IX_adjustments_warehouse_id").OnTable("adjustments");
        Delete.Index("IX_quotations_warehouse_id").OnTable("quotations");
        Delete.Index("IX_expenses_warehouse_id").OnTable("expenses");
        Delete.Index("IX_purchases_warehouse_id").OnTable("purchases");
        Delete.Index("IX_sales_warehouse_id").OnTable("sales");

        // Remove PK from user_warehouse
        Delete.PrimaryKey("PK_user_warehouse").FromTable("user_warehouse");

        // Restore settings tables
        Rename.Table("payment_gateway_settings_old").To("payment_gateway_settings");
        Rename.Table("sms_settings_old").To("sms_settings");
        Rename.Table("mail_settings_old").To("mail_settings");

        // Restore settings columns
        Delete.Column("currency_name").FromTable("settings");
        Delete.Column("currency_symbol").FromTable("settings");
        Delete.Column("currency_code").FromTable("settings");

        Delete.Column("siat_token").FromTable("settings");
        Delete.Column("siat_certificate").FromTable("settings");
        Delete.Column("siat_environment").FromTable("settings");
        Delete.Column("siat_modality").FromTable("settings");
        Delete.Column("siat_emission_type").FromTable("settings");

        Delete.Column("payment_description").FromTable("settings");
        Delete.Column("payment_is_active").FromTable("settings");
        Delete.Column("payment_api_secret").FromTable("settings");
        Delete.Column("payment_api_key").FromTable("settings");
        Delete.Column("payment_gateway_name").FromTable("settings");
        Delete.Column("sms_description").FromTable("settings");
        Delete.Column("sms_is_active").FromTable("settings");
        Delete.Column("sms_sender_id").FromTable("settings");
        Delete.Column("sms_api_secret").FromTable("settings");
        Delete.Column("sms_api_key").FromTable("settings");
        Delete.Column("sms_gateway_name").FromTable("settings");
        Delete.Column("mail_from_name").FromTable("settings");
        Delete.Column("mail_from_address").FromTable("settings");
        Delete.Column("mail_encryption").FromTable("settings");
        Delete.Column("mail_password").FromTable("settings");
        Delete.Column("mail_username").FromTable("settings");
        Delete.Column("mail_port").FromTable("settings");
        Delete.Column("mail_host").FromTable("settings");

        // Restore currencies table
        Rename.Table("currencies_old").To("currencies");
        Rename.Table("CurrencySettings_old").To("CurrencySettings");

        // Restore users table
        Delete.ForeignKey().FromTable("users").ForeignColumn("role_id").ToTable("roles").PrimaryColumn("id");
        Delete.Column("role_id").FromTable("users");
        Delete.Column("default_warehouse_id").FromTable("users");
        Rename.Column("role_old").OnTable("users").To("role");
    }
}
