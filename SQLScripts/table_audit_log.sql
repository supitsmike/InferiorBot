-- Table: public.audit_log

-- DROP TABLE IF EXISTS public.audit_log;

CREATE TABLE IF NOT EXISTS public.audit_log
(
    log_id uuid NOT NULL DEFAULT uuid_generate_v4(),
    user_id numeric(19,0) NOT NULL,
    table_name character varying(64) COLLATE pg_catalog."default" NOT NULL,
    column_name character varying(64) COLLATE pg_catalog."default" NOT NULL,
    previous_data text COLLATE pg_catalog."default" NOT NULL,
    new_data text COLLATE pg_catalog."default" NOT NULL,
    "timestamp" timestamp without time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT audit_log_pkey PRIMARY KEY (log_id),
    CONSTRAINT audit_log_user_id_fkey FOREIGN KEY (user_id)
        REFERENCES public.users (user_id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public.audit_log
    OWNER to inferior_user;