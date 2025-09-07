-- Table: public.users

-- DROP TABLE IF EXISTS public.users;

CREATE TABLE IF NOT EXISTS public.users
(
    user_id text COLLATE pg_catalog."default" NOT NULL,
    balance money NOT NULL DEFAULT 100,
    banned boolean NOT NULL DEFAULT false,
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

-- Trigger: create_audit_log_partition

-- DROP TRIGGER IF EXISTS create_audit_log_partition ON public.users;

CREATE OR REPLACE TRIGGER create_audit_log_partition
    BEFORE INSERT
    ON public.users
    FOR EACH ROW
    EXECUTE FUNCTION public.create_audit_log_partition();