-- FUNCTION: public.audit_users_changes()

-- DROP FUNCTION IF EXISTS public.audit_users_changes();

CREATE OR REPLACE FUNCTION public.audit_users_changes()
    RETURNS trigger
    LANGUAGE 'plpgsql'
    COST 100
    VOLATILE NOT LEAKPROOF
AS $BODY$
BEGIN
    IF TG_OP = 'UPDATE' THEN
		-- balance
		IF NEW.balance IS DISTINCT FROM OLD.balance THEN
        	INSERT INTO audit_log (
        	    user_id,
        	    table_name,
        	    column_name,
        	    previous_data,
        	    new_data
        	) VALUES (
        	    OLD.user_id,
        	    'users',
        	    'balance',
        	    OLD.balance,
        	    NEW.balance
        	);
		END IF;
		
		-- level
		IF NEW.level IS DISTINCT FROM OLD.level THEN
        	INSERT INTO audit_log (
        	    user_id,
        	    table_name,
        	    column_name,
        	    previous_data,
        	    new_data
        	) VALUES (
        	    OLD.user_id,
        	    'users',
        	    'level',
        	    OLD.level,
        	    NEW.level
        	);
		END IF;
		
		-- xp
		IF NEW.xp IS DISTINCT FROM OLD.xp THEN
        	INSERT INTO audit_log (
        	    user_id,
        	    table_name,
        	    column_name,
        	    previous_data,
        	    new_data
        	) VALUES (
        	    OLD.user_id,
        	    'users',
        	    'xp',
        	    OLD.xp,
        	    NEW.xp
        	);
		END IF;
		
		-- job_id
		IF NEW.job_id IS DISTINCT FROM OLD.job_id THEN
        	INSERT INTO audit_log (
        	    user_id,
        	    table_name,
        	    column_name,
        	    previous_data,
        	    new_data
        	) VALUES (
        	    OLD.user_id,
        	    'users',
        	    'job_id',
        	    OLD.job_id,
        	    NEW.job_id
        	);
		END IF;
    END IF;
	RETURN NEW;
END;
$BODY$;

ALTER FUNCTION public.audit_users_changes()
    OWNER TO inferior_user;
