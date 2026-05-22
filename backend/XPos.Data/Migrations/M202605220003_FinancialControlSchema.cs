using FluentMigrator;

namespace XPos.Data.Migrations;

[Migration(202605220003)]
public class M202605220003_FinancialControlSchema : Migration
{
    public override void Up()
    {
        // 1. cash_registers
        Create.Table("cash_registers")
            .WithColumn("id").AsInt64().PrimaryKey().Identity()
            .WithColumn("name").AsString(255).NotNullable()
            .WithColumn("warehouse_id").AsInt64().NotNullable().ForeignKey("warehouses", "id")
            .WithColumn("is_active").AsBoolean().NotNullable().WithDefaultValue(true)
            .WithColumn("is_matriz").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("created_at").AsDateTime().Nullable()
            .WithColumn("updated_at").AsDateTime().Nullable()
            .WithColumn("deleted_at").AsDateTime().Nullable();

        // 2. cash_shifts
        Create.Table("cash_shifts")
            .WithColumn("id").AsInt64().PrimaryKey().Identity()
            .WithColumn("cash_register_id").AsInt64().NotNullable().ForeignKey("cash_registers", "id")
            .WithColumn("user_id").AsInt64().NotNullable().ForeignKey("users", "id")
            .WithColumn("status").AsString(50).NotNullable().WithDefaultValue("OPEN")
            .WithColumn("opened_at").AsDateTime().NotNullable()
            .WithColumn("closed_at").AsDateTime().Nullable()
            .WithColumn("starting_cash").AsDecimal(18, 2).NotNullable()
            .WithColumn("ending_cash_expected").AsDecimal(18, 2).NotNullable()
            .WithColumn("ending_cash_actual").AsDecimal(18, 2).Nullable()
            .WithColumn("discrepancy").AsDecimal(18, 2).Nullable()
            .WithColumn("closing_notes").AsCustom("TEXT").Nullable();

        // 3. cash_transactions
        Create.Table("cash_transactions")
            .WithColumn("id").AsInt64().PrimaryKey().Identity()
            .WithColumn("cash_shift_id").AsInt64().NotNullable().ForeignKey("cash_shifts", "id")
            .WithColumn("voucher_number").AsString(100).NotNullable().Unique()
            .WithColumn("transaction_type").AsString(50).NotNullable()
            .WithColumn("amount").AsDecimal(18, 2).NotNullable()
            .WithColumn("notes").AsCustom("TEXT").Nullable()
            .WithColumn("recipient_name").AsString(255).Nullable()
            .WithColumn("created_by").AsInt64().NotNullable().ForeignKey("users", "id")
            .WithColumn("created_at").AsDateTime().NotNullable();

        // 4. Alter sales table to add cash_shift_id
        if (!Schema.Table("sales").Column("cash_shift_id").Exists())
        {
            Alter.Table("sales")
                .AddColumn("cash_shift_id").AsInt64().Nullable().ForeignKey("cash_shifts", "id");
        }

        // 5. Seed default cash register for each warehouse
        Execute.Sql(@"
            INSERT INTO cash_registers (name, warehouse_id, is_active, is_matriz, created_at)
            SELECT 'Caja Principal - ' || name, id, true, false, CURRENT_TIMESTAMP
            FROM warehouses;
        ");
    }

    public override void Down()
    {
        if (Schema.Table("sales").Column("cash_shift_id").Exists())
        {
            Delete.ForeignKey("FK_sales_cash_shift_id_cash_shifts_id").OnTable("sales");
            Delete.Column("cash_shift_id").FromTable("sales");
        }

        Delete.Table("cash_transactions");
        Delete.Table("cash_shifts");
        Delete.Table("cash_registers");
    }
}
