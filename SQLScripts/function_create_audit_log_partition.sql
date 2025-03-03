-- FUNCTION: public.create_audit_log_partition()

-- DROP FUNCTION IF EXISTS public.create_audit_log_partition();

CREATE OR REPLACE FUNCTION public.create_audit_log_partition()
    RETURNS trigger
    LANGUAGE 'plpgsql'
    COST 100
    VOLATILE NOT LEAKPROOF
AS $BODY$
DECLARE
  partition_name TEXT;
BEGIN
  partition_name := 'audit_log_' || NEW.user_id;
  
  IF NOT EXISTS (
    SELECT 1
    FROM pg_catalog.pg_class c
    WHERE c.relname = partition_name
  ) THEN
    EXECUTE 'CREATE TABLE IF NOT EXISTS "' || partition_name || '" (LIKE audit_log INCLUDING ALL)';
    EXECUTE 'ALTER TABLE audit_log ATTACH PARTITION "' || partition_name || '" FOR VALUES IN (' || NEW.user_id || ')';
  END IF;
  
  RETURN NEW;
END;
$BODY$;

ALTER FUNCTION public.create_audit_log_partition()
    OWNER to inferior_user;
