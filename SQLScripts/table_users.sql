-- Table: public.users

-- DROP TABLE IF EXISTS public.users;

CREATE TABLE IF NOT EXISTS public.users
(
    user_id numeric(19,0) NOT NULL,
    balance money NOT NULL DEFAULT 100,
    banned boolean NOT NULL DEFAULT false,
    level numeric(4,0) NOT NULL DEFAULT 1,
    xp integer NOT NULL DEFAULT 0,
    job_id integer,
    daily_cooldown timestamp without time zone,
    work_cooldown timestamp without time zone,
    CONSTRAINT users_pkey PRIMARY KEY (user_id),
    CONSTRAINT users_user_id_key UNIQUE (user_id)
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public.users
    OWNER to inferior_user;

-- Trigger: audit_users_changes

-- DROP TRIGGER IF EXISTS audit_users_changes ON public.users;

CREATE OR REPLACE TRIGGER audit_users_changes
    BEFORE UPDATE 
    ON public.users
    FOR EACH ROW
    EXECUTE FUNCTION public.audit_users_changes();