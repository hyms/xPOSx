using FluentMigrator;

namespace XPos.Data.Migrations;

[Migration(202605220001)]
public class M202605220001_AddWarehousesAllAccessPermission : Migration
{
    public override void Up()
    {
        // 1. Insert warehouses_all_access permission if not exists
        Execute.Sql(@"
            INSERT INTO permissions (name, guard_name) 
            SELECT 'warehouses_all_access', 'web' 
            WHERE NOT EXISTS (SELECT 1 FROM permissions WHERE name = 'warehouses_all_access');");

        // 2. Assign warehouses_all_access permission to Admin Role (ID 1) if not exists
        Execute.Sql(@"
            INSERT INTO role_has_permissions (role_id, permission_id) 
            SELECT 1, id FROM permissions 
            WHERE name = 'warehouses_all_access' 
            AND NOT EXISTS (
                SELECT 1 FROM role_has_permissions 
                WHERE role_id = 1 
                AND permission_id = (SELECT id FROM permissions WHERE name = 'warehouses_all_access')
            );");
    }

    public override void Down()
    {
        Execute.Sql(@"
            DELETE FROM role_has_permissions 
            WHERE role_id = 1 
            AND permission_id = (SELECT id FROM permissions WHERE name = 'warehouses_all_access');");

        Execute.Sql("DELETE FROM permissions WHERE name = 'warehouses_all_access';");
    }
}
