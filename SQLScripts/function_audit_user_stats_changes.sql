-- FUNCTION: public.audit_user_stats_changes()

-- DROP FUNCTION IF EXISTS public.audit_user_stats_changes();

CREATE OR REPLACE FUNCTION public.audit_user_stats_changes()
    RETURNS trigger
    LANGUAGE 'plpgsql'
    COST 100
    VOLATILE NOT LEAKPROOF
AS $BODY$
BEGIN
    IF TG_OP = 'UPDATE' THEN
		-- all_time_won
		IF NEW.all_time_won IS DISTINCT FROM OLD.all_time_won THEN
        	INSERT INTO audit_log (
        	    user_id,
        	    table_name,
        	    column_name,
        	    previous_data,
        	    new_data
        	) VALUES (
        	    OLD.user_id,
        	    'user_stats',
        	    'all_time_won',
        	    OLD.all_time_won,
        	    NEW.all_time_won
        	);
		
		-- all_time_lost
		ELSIF NEW.all_time_lost IS DISTINCT FROM OLD.all_time_lost THEN
        	INSERT INTO audit_log (
        	    user_id,
        	    table_name,
        	    column_name,
        	    previous_data,
        	    new_data
        	) VALUES (
        	    OLD.user_id,
        	    'user_stats',
        	    'all_time_lost',
        	    OLD.all_time_lost,
        	    NEW.all_time_lost
        	);
		
		-- biggest_win
		ELSIF NEW.biggest_win IS DISTINCT FROM OLD.biggest_win THEN
        	INSERT INTO audit_log (
        	    user_id,
        	    table_name,
        	    column_name,
        	    previous_data,
        	    new_data
        	) VALUES (
        	    OLD.user_id,
        	    'user_stats',
        	    'biggest_win',
        	    OLD.biggest_win,
        	    NEW.biggest_win
        	);
		
		-- biggest_loss
		ELSIF NEW.biggest_loss IS DISTINCT FROM OLD.biggest_loss THEN
        	INSERT INTO audit_log (
        	    user_id,
        	    table_name,
        	    column_name,
        	    previous_data,
        	    new_data
        	) VALUES (
        	    OLD.user_id,
        	    'user_stats',
        	    'biggest_loss',
        	    OLD.biggest_loss,
        	    NEW.biggest_loss
        	);
		
		-- daily_count
		ELSIF NEW.daily_count IS DISTINCT FROM OLD.daily_count THEN
        	INSERT INTO audit_log (
        	    user_id,
        	    table_name,
        	    column_name,
        	    previous_data,
        	    new_data
        	) VALUES (
        	    OLD.user_id,
        	    'user_stats',
        	    'daily_count',
        	    OLD.daily_count,
        	    NEW.daily_count
        	);
		
		-- daily_streak
		ELSIF NEW.daily_streak IS DISTINCT FROM OLD.daily_streak THEN
        	INSERT INTO audit_log (
        	    user_id,
        	    table_name,
        	    column_name,
        	    previous_data,
        	    new_data
        	) VALUES (
        	    OLD.user_id,
        	    'user_stats',
        	    'daily_streak',
        	    OLD.daily_streak,
        	    NEW.daily_streak
        	);
		
		-- work_count
		ELSIF NEW.work_count IS DISTINCT FROM OLD.work_count THEN
        	INSERT INTO audit_log (
        	    user_id,
        	    table_name,
        	    column_name,
        	    previous_data,
        	    new_data
        	) VALUES (
        	    OLD.user_id,
        	    'user_stats',
        	    'work_count',
        	    OLD.work_count,
        	    NEW.work_count
        	);
		
		-- coin_flip_wins
		ELSIF NEW.coin_flip_wins IS DISTINCT FROM OLD.coin_flip_wins THEN
        	INSERT INTO audit_log (
        	    user_id,
        	    table_name,
        	    column_name,
        	    previous_data,
        	    new_data
        	) VALUES (
        	    OLD.user_id,
        	    'user_stats',
        	    'coin_flip_wins',
        	    OLD.coin_flip_wins,
        	    NEW.coin_flip_wins
        	);
		
		-- coin_flip_losses
		ELSIF NEW.coin_flip_losses IS DISTINCT FROM OLD.coin_flip_losses THEN
        	INSERT INTO audit_log (
        	    user_id,
        	    table_name,
        	    column_name,
        	    previous_data,
        	    new_data
        	) VALUES (
        	    OLD.user_id,
        	    'user_stats',
        	    'coin_flip_losses',
        	    OLD.coin_flip_losses,
        	    NEW.coin_flip_losses
        	);
		END IF;
    END IF;
	RETURN NEW;
END;
$BODY$;

ALTER FUNCTION public.audit_user_stats_changes()
    OWNER TO inferior_user;
