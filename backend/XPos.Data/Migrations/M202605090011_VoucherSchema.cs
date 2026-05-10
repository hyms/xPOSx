using FluentMigrator;

namespace XPos.Data.Migrations;

[Migration(202605090011)]
public class M202605090011_VoucherSchema : Migration
{
    public override void Up()
    {
        Create.Table("vouchers")
            .WithColumn("id").AsInt64().PrimaryKey().Identity()
            .WithColumn("sale_id").AsInt64().ForeignKey("sales", "id").NotNullable()
            .WithColumn("voucher_type").AsString(50).NotNullable() // e.g., 'Factura A', 'Ticket B'
            .WithColumn("voucher_number").AsString(100).NotNullable().Unique() // Prefijo-Numero, e.g., '0001-00001234'
            .WithColumn("cae").AsString(100).NotNullable() // Fiscal authorization code
            .WithColumn("cae_expiration").AsDate().NotNullable()
            .WithColumn("issued_at").AsDateTime().NotNullable()
            .WithAuditFields();

        Alter.Table("sales")
            .AddColumn("voucher_id").AsInt64().Nullable().ForeignKey("vouchers", "id");
    }

    public override void Down()
    {
        Delete.Column("voucher_id").FromTable("sales");
        Delete.Table("vouchers");
    }
}
