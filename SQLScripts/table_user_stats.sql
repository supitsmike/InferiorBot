-- Table: public.user_stats

-- DROP TABLE IF EXISTS public.user_stats;

CREATE TABLE IF NOT EXISTS public.user_stats
(
    user_id text COLLATE pg_catalog."default" NOT NULL,
    all_time_won money NOT NULL DEFAULT 0,
    all_time_lost money NOT NULL DEFAULT 0,
    biggest_win money NOT NULL DEFAULT 0,
    biggest_loss money NOT NULL DEFAULT 0,
    daily_count numeric(10,0) NOT NULL DEFAULT 0,
    daily_streak numeric(10,0) NOT NULL DEFAULT 0,
    work_count numeric(10,0) NOT NULL DEFAULT 0,
    coin_flip_wins numeric(10,0) NOT NULL DEFAULT 0,
    coin_flip_losses numeric(10,0) NOT NULL DEFAULT 0,
    guess_wins numeric(10,0) NOT NULL DEFAULT 0,
    guess_losses numeric(10,0) NOT NULL DEFAULT 0,
    ride_the_bus_wins numeric(10,0) NOT NULL DEFAULT 0,
    ride_the_bus_losses numeric(10,0) NOT NULL DEFAULT 0,
    CONSTRAINT user_stats_pkey PRIMARY KEY (user_id),
    CONSTRAINT user_stats_user_id_fkey FOREIGN KEY (user_id)
        REFERENCES public.users (user_id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public.user_stats
    OWNER to inferior_user;

-- Trigger: audit_user_stats_changes

-- DROP TRIGGER IF EXISTS audit_user_stats_changes ON public.user_stats;

CREATE OR REPLACE TRIGGER audit_user_stats_changes
    BEFORE UPDATE 
    ON public.user_stats
    FOR EACH ROW
    EXECUTE FUNCTION public.audit_user_stats_changes();