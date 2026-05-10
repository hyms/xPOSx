using FluentMigrator;

namespace XPos.Data.Migrations;

[Migration(202605090010)]
public class M202605090010_SeedInitialData : Migration
{
    public override void Up()
    {
        var adminPasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123");

        // Roles
        Execute.Sql("INSERT INTO roles (name, guard_name) VALUES ('Admin', 'Admin')");
        Execute.Sql("INSERT INTO roles (name, guard_name) VALUES ('Vendor', 'Vendor')");
        Execute.Sql("INSERT INTO roles (name, guard_name) VALUES ('Design', 'Design')");

        // Permissions
        var modules = new[] { 
            "users", "roles", "permissions", "products", "sales", "warehouses", 
            "clients", "providers", "categories", "units", "purchases", 
            "transfers", "adjustments", "expenses", "returns", "reports", 
            "quotations", "settings", "expense_categories"
        };

        foreach (var module in modules)
        {
            Insert.IntoTable("permissions").Row(new { name = $"{module}_view", guard_name = "web" });
            Insert.IntoTable("permissions").Row(new { name = $"{module}_create", guard_name = "web" });
            Insert.IntoTable("permissions").Row(new { name = $"{module}_edit", guard_name = "web" });
            Insert.IntoTable("permissions").Row(new { name = $"{module}_delete", guard_name = "web" });
        }

        // Special permissions
        Insert.IntoTable("permissions").Row(new { name = "pos_view", guard_name = "web" });
        Insert.IntoTable("permissions").Row(new { name = "inventory_view", guard_name = "web" });
        Insert.IntoTable("permissions").Row(new { name = "reports_profit", guard_name = "web" });
        Insert.IntoTable("permissions").Row(new { name = "reports_stock", guard_name = "web" });

        // Assign all permissions to Admin Role (ID 1)
        Execute.Sql("INSERT INTO role_has_permissions (role_id, permission_id) SELECT 1, id FROM permissions");

        // Admin User
        Execute.Sql($@"
            INSERT INTO users (username, password, email, first_name, last_name, role, is_active, created_at)
            VALUES ('admin', '{adminPasswordHash}', 'admin@example.com', 'System', 'Admin', 1, true, NOW())");

        // Initial Settings
        Execute.Sql(@"
            INSERT INTO settings (company_name, email, version, days, created_at)
            VALUES ('xPOSx System', 'contact@xposx.com', '1.0.0', 365, NOW())");
    }

    public override void Down()
    {
        Execute.Sql("DELETE FROM settings");
        Execute.Sql("DELETE FROM users");
        Execute.Sql("DELETE FROM role_has_permissions");
        Execute.Sql("DELETE FROM permissions");
        Execute.Sql("DELETE FROM roles");
    }
}
