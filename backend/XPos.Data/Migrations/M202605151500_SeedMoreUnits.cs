using FluentMigrator;

namespace XPos.Data.Migrations;

[Migration(202605151500)]
public class M202605151500_SeedMoreUnits : Migration
{
    public override void Up()
    {
        // Inserting common units
        Insert.IntoTable("units").Row(new { name = "Unidad", short_name = "Unid", created_at = DateTime.Now });
        Insert.IntoTable("units").Row(new { name = "Pieza", short_name = "Pz", created_at = DateTime.Now });
        Insert.IntoTable("units").Row(new { name = "Kilogramo", short_name = "Kg", created_at = DateTime.Now });
        Insert.IntoTable("units").Row(new { name = "Gramo", short_name = "g", base_unit = 3, @operator = "/", operator_value = 1000, created_at = DateTime.Now });
        Insert.IntoTable("units").Row(new { name = "Litro", short_name = "Lt", created_at = DateTime.Now });
        Insert.IntoTable("units").Row(new { name = "Mililitro", short_name = "ml", base_unit = 5, @operator = "/", operator_value = 1000, created_at = DateTime.Now });
        Insert.IntoTable("units").Row(new { name = "Servicio", short_name = "Serv", created_at = DateTime.Now });
        Insert.IntoTable("units").Row(new { name = "Paquete", short_name = "Paq", created_at = DateTime.Now });
    }

    public override void Down()
    {
        Execute.Sql("DELETE FROM units WHERE short_name IN ('Unid', 'Pz', 'Kg', 'g', 'Lt', 'ml', 'Serv', 'Paq')");
    }
}
