using FluentMigrator;

namespace XPos.Data.Migrations;

[Migration(202605230002)]
public class M202605230002_CreateAuditLogsTable : Migration
{
    public override void Up()
    {
        Execute.Sql(@"
            CREATE TABLE IF NOT EXISTS public.audit_logs (
                id BIGSERIAL PRIMARY KEY,
                user_id BIGINT,
                action VARCHAR(50) NOT NULL,
                table_name VARCHAR(100) NOT NULL,
                record_id BIGINT NOT NULL,
                old_values JSONB,
                new_values JSONB,
                ip_address VARCHAR(45),
                user_agent VARCHAR(1000),
                created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
            );

            CREATE OR REPLACE FUNCTION public.proc_audit_logger()
            RETURNS TRIGGER AS $$
            DECLARE
                v_user_id BIGINT;
                v_ip_address VARCHAR(45);
                v_user_agent VARCHAR(1000);
                v_record_id BIGINT;
                v_old_values JSONB := NULL;
                v_new_values JSONB := NULL;
                v_user_id_str TEXT;
            BEGIN
                v_user_id_str := current_setting('audit.user_id', true);
                IF v_user_id_str IS NOT NULL AND v_user_id_str <> '' THEN
                    v_user_id := v_user_id_str::BIGINT;
                ELSE
                    v_user_id := NULL;
                END IF;

                v_ip_address := current_setting('audit.ip_address', true);
                v_user_agent := current_setting('audit.user_agent', true);

                IF TG_OP = 'INSERT' THEN
                    v_record_id := NEW.id;
                    v_new_values := to_jsonb(NEW);
                ELSIF TG_OP = 'UPDATE' THEN
                    v_record_id := NEW.id;
                    v_old_values := to_jsonb(OLD);
                    v_new_values := to_jsonb(NEW);
                ELSIF TG_OP = 'DELETE' THEN
                    v_record_id := OLD.id;
                    v_old_values := to_jsonb(OLD);
                END IF;

                INSERT INTO public.audit_logs (
                    user_id,
                    action,
                    table_name,
                    record_id,
                    old_values,
                    new_values,
                    ip_address,
                    user_agent,
                    created_at
                ) VALUES (
                    v_user_id,
                    TG_OP,
                    TG_TABLE_NAME,
                    v_record_id,
                    v_old_values,
                    v_new_values,
                    COALESCE(v_ip_address, 'unknown'),
                    COALESCE(v_user_agent, 'unknown'),
                    CURRENT_TIMESTAMP
                );

                IF TG_OP = 'DELETE' THEN
                    RETURN OLD;
                ELSE
                    RETURN NEW;
                END IF;
            END;
            $$ LANGUAGE plpgsql;

            -- sales trigger
            DROP TRIGGER IF EXISTS t_sales_audit ON public.sales;
            CREATE TRIGGER t_sales_audit
            AFTER INSERT OR UPDATE OR DELETE ON public.sales
            FOR EACH ROW EXECUTE FUNCTION public.proc_audit_logger();

            -- cash_shifts trigger
            DROP TRIGGER IF EXISTS t_cash_shifts_audit ON public.cash_shifts;
            CREATE TRIGGER t_cash_shifts_audit
            AFTER INSERT OR UPDATE OR DELETE ON public.cash_shifts
            FOR EACH ROW EXECUTE FUNCTION public.proc_audit_logger();

            -- cms_pages trigger
            DROP TRIGGER IF EXISTS t_cms_pages_audit ON public.cms_pages;
            CREATE TRIGGER t_cms_pages_audit
            AFTER INSERT OR UPDATE OR DELETE ON public.cms_pages
            FOR EACH ROW EXECUTE FUNCTION public.proc_audit_logger();

            -- expenses trigger
            DROP TRIGGER IF EXISTS t_expenses_audit ON public.expenses;
            CREATE TRIGGER t_expenses_audit
            AFTER INSERT OR UPDATE OR DELETE ON public.expenses
            FOR EACH ROW EXECUTE FUNCTION public.proc_audit_logger();

            -- products trigger
            DROP TRIGGER IF EXISTS t_products_audit ON public.products;
            CREATE TRIGGER t_products_audit
            AFTER INSERT OR UPDATE OR DELETE ON public.products
            FOR EACH ROW EXECUTE FUNCTION public.proc_audit_logger();
        ");
    }

    public override void Down()
    {
        Execute.Sql(@"
            DROP TRIGGER IF EXISTS t_sales_audit ON public.sales;
            DROP TRIGGER IF EXISTS t_cash_shifts_audit ON public.cash_shifts;
            DROP TRIGGER IF EXISTS t_cms_pages_audit ON public.cms_pages;
            DROP TRIGGER IF EXISTS t_expenses_audit ON public.expenses;
            DROP TRIGGER IF EXISTS t_products_audit ON public.products;
            DROP FUNCTION IF EXISTS public.proc_audit_logger();
            DROP TABLE IF EXISTS public.audit_logs;
        ");
    }
}
