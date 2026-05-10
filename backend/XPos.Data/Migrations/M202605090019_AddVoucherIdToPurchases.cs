using FluentMigrator;

namespace XPos.Data.Migrations;

[Migration(202605090019)]
public class M202605090019_AddVoucherIdToPurchases : Migration
{
    public override void Up()
    {
        Alter.Table("purchases")
            .AddColumn("voucher_id").AsInt64().Nullable().ForeignKey("vouchers", "id");
    }

    public override void Down()
    {
        Delete.Column("voucher_id").FromTable("purchases");
    }
}
