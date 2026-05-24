using FluentMigrator;

namespace XPos.Data.Migrations;

[Migration(202605230001)]
public class M202605230001_AddCmsAndOnlineSalesFields : Migration
{
    public override void Up()
    {
        // 1. Create cms_pages table
        if (!Schema.Table("cms_pages").Exists())
        {
            Create.Table("cms_pages")
                .WithColumn("id").AsInt64().PrimaryKey().Identity()
                .WithColumn("title").AsString(255).NotNullable()
                .WithColumn("slug").AsString(255).NotNullable().Unique()
                .WithColumn("content").AsCustom("TEXT").NotNullable()
                .WithAuditFields();
        }

        // 2. Add columns to sales table if they do not exist
        if (!Schema.Table("sales").Column("payment_receipt_path").Exists())
        {
            Alter.Table("sales").AddColumn("payment_receipt_path").AsString(1000).Nullable();
        }

        if (!Schema.Table("sales").Column("nit").Exists())
        {
            Alter.Table("sales").AddColumn("nit").AsString(100).Nullable();
        }

        if (!Schema.Table("sales").Column("razon_social").Exists())
        {
            Alter.Table("sales").AddColumn("razon_social").AsString(255).Nullable();
        }

        // 3. Add qr_code_path to settings table if it does not exist
        if (!Schema.Table("settings").Column("qr_code_path").Exists())
        {
            Alter.Table("settings").AddColumn("qr_code_path").AsString(1000).Nullable();
        }

        // 4. Seed terminos-y-condiciones page if not exists
        Execute.Sql(@"
            INSERT INTO cms_pages (title, slug, content, created_at)
            SELECT 'Términos y Condiciones', 'terminos-y-condiciones', '<h2>Términos y Condiciones de Uso</h2><p>Bienvenido a nuestro portal público de ventas online xPOSx. Al realizar un pedido de preventa QR, usted acepta y se compromete a cumplir con los presentes términos y condiciones.</p><p><b>1. Proceso de Pago:</b> El cliente debe escanear el código QR proporcionado, transferir el monto exacto y subir una captura clara del comprobante de pago.</p><p><b>2. Verificación:</b> Los pedidos web quedan bajo el estado <i>Pendiente de Verificación</i> hasta que el administrador del POS apruebe la transacción y valide el depósito en la cuenta.</p><p><b>3. Disponibilidad de Stock:</b> La reserva se confirma únicamente tras la aprobación del pedido.</p>', CURRENT_TIMESTAMP
            WHERE NOT EXISTS (SELECT 1 FROM cms_pages WHERE slug = 'terminos-y-condiciones');
        ");
    }

    public override void Down()
    {
        if (Schema.Table("sales").Column("payment_receipt_path").Exists())
        {
            Delete.Column("payment_receipt_path").FromTable("sales");
        }

        if (Schema.Table("sales").Column("nit").Exists())
        {
            Delete.Column("nit").FromTable("sales");
        }

        if (Schema.Table("sales").Column("razon_social").Exists())
        {
            Delete.Column("razon_social").FromTable("sales");
        }

        if (Schema.Table("settings").Column("qr_code_path").Exists())
        {
            Delete.Column("qr_code_path").FromTable("settings");
        }

        Delete.Table("cms_pages");
    }
}
