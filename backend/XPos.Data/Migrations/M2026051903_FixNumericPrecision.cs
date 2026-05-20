using FluentMigrator;

namespace XPos.Data.Migrations;

[Migration(2026051903)]
public class M2026051903_FixNumericPrecision : Migration
{
    public override void Up()
    {
        // 1. Fix payment_sales (the table was incorrectly named sale_payments in the previous migration)
        if (Schema.Table("payment_sales").Exists())
        {
            Alter.Table("payment_sales").AlterColumn("amount").AsDecimal(18, 2);
            Alter.Table("payment_sales").AlterColumn("change").AsDecimal(18, 2);
        }

        // 2. Fix transfers table (omitted in previous migration)
        if (Schema.Table("transfers").Exists())
        {
            Alter.Table("transfers").AlterColumn("tax_rate").AsDecimal(18, 2);
            Alter.Table("transfers").AlterColumn("tax_net").AsDecimal(18, 2);
            Alter.Table("transfers").AlterColumn("discount").AsDecimal(18, 2);
            Alter.Table("transfers").AlterColumn("shipping").AsDecimal(18, 2);
            Alter.Table("transfers").AlterColumn("grand_total").AsDecimal(18, 2);
        }

        // 3. Fix transfer_details table (omitted in previous migration)
        if (Schema.Table("transfer_details").Exists())
        {
            Alter.Table("transfer_details").AlterColumn("cost").AsDecimal(18, 2);
            Alter.Table("transfer_details").AlterColumn("tax_net").AsDecimal(18, 2);
            Alter.Table("transfer_details").AlterColumn("discount").AsDecimal(18, 2);
            Alter.Table("transfer_details").AlterColumn("total").AsDecimal(18, 2);
        }
    }

    public override void Down()
    {
        // 1. Revert payment_sales
        if (Schema.Table("payment_sales").Exists())
        {
            Alter.Table("payment_sales").AlterColumn("amount").AsFloat();
            Alter.Table("payment_sales").AlterColumn("change").AsFloat();
        }

        // 2. Revert transfers
        if (Schema.Table("transfers").Exists())
        {
            Alter.Table("transfers").AlterColumn("tax_rate").AsFloat();
            Alter.Table("transfers").AlterColumn("tax_net").AsFloat();
            Alter.Table("transfers").AlterColumn("discount").AsFloat();
            Alter.Table("transfers").AlterColumn("shipping").AsFloat();
            Alter.Table("transfers").AlterColumn("grand_total").AsFloat();
        }

        // 3. Revert transfer_details
        if (Schema.Table("transfer_details").Exists())
        {
            Alter.Table("transfer_details").AlterColumn("cost").AsFloat();
            Alter.Table("transfer_details").AlterColumn("tax_net").AsFloat();
            Alter.Table("transfer_details").AlterColumn("discount").AsFloat();
            Alter.Table("transfer_details").AlterColumn("total").AsFloat();
        }
    }
}
