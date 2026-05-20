using FluentMigrator;

namespace XPos.Data.Migrations;

[Migration(2026051902)]
public class M2026051902_AddInvoiceNumberToSettings : Migration
{
    public override void Up()
    {
        Alter.Table("settings")
            .AddColumn("invoice_number").AsInt32().WithDefaultValue(0);
    }

    public override void Down()
    {
        Delete.Column("invoice_number").FromTable("settings");
    }
}
