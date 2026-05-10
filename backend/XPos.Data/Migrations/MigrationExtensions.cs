using FluentMigrator;
using FluentMigrator.Builders.Create.Table;

namespace XPos.Data.Migrations;

public static class MigrationExtensions
{
    public static ICreateTableColumnOptionOrWithColumnSyntax WithAuditFields(this ICreateTableWithColumnSyntax builder)
    {
        return builder
            .WithColumn("created_at").AsDateTime().Nullable()
            .WithColumn("updated_at").AsDateTime().Nullable()
            .WithColumn("deleted_at").AsDateTime().Nullable()
            .WithColumn("created_by").AsInt64().Nullable().ForeignKey("users", "id")
            .WithColumn("updated_by").AsInt64().Nullable().ForeignKey("users", "id")
            .WithColumn("deleted_by").AsInt64().Nullable().ForeignKey("users", "id");
    }
}
