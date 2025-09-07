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
		END IF;
		
		-- all_time_lost
		IF NEW.all_time_lost IS DISTINCT FROM OLD.all_time_lost THEN
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
		END IF;
		
		-- biggest_win
		IF NEW.biggest_win IS DISTINCT FROM OLD.biggest_win THEN
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
		END IF;
		
		-- biggest_loss
		IF NEW.biggest_loss IS DISTINCT FROM OLD.biggest_loss THEN
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
		END IF;
		
		-- daily_count
		IF NEW.daily_count IS DISTINCT FROM OLD.daily_count THEN
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
		END IF;
		
		-- daily_streak
		IF NEW.daily_streak IS DISTINCT FROM OLD.daily_streak THEN
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
		END IF;
		
		-- work_count
		IF NEW.work_count IS DISTINCT FROM OLD.work_count THEN
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
		END IF;
		
		-- coin_flip_wins
		IF NEW.coin_flip_wins IS DISTINCT FROM OLD.coin_flip_wins THEN
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
		END IF;
		
		-- coin_flip_losses
		IF NEW.coin_flip_losses IS DISTINCT FROM OLD.coin_flip_losses THEN
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
		
		-- guess_wins
		IF NEW.guess_wins IS DISTINCT FROM OLD.guess_wins THEN
        	INSERT INTO audit_log (
        	    user_id,
        	    table_name,
        	    column_name,
        	    previous_data,
        	    new_data
        	) VALUES (
        	    OLD.user_id,
        	    'user_stats',
        	    'guess_wins',
        	    OLD.guess_wins,
        	    NEW.guess_wins
        	);
		END IF;
		
		-- guess_losses
		IF NEW.guess_losses IS DISTINCT FROM OLD.guess_losses THEN
        	INSERT INTO audit_log (
        	    user_id,
        	    table_name,
        	    column_name,
        	    previous_data,
        	    new_data
        	) VALUES (
        	    OLD.user_id,
        	    'user_stats',
        	    'guess_losses',
        	    OLD.guess_losses,
        	    NEW.guess_losses
        	);
		END IF;
    END IF;
	RETURN NEW;
END;
$BODY$;

ALTER FUNCTION public.audit_user_stats_changes()
    OWNER TO inferior_user;
