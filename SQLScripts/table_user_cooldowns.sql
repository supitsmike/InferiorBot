-- Table: public.user_cooldowns

-- DROP TABLE IF EXISTS public.user_cooldowns;

CREATE TABLE IF NOT EXISTS public.user_cooldowns
(
    user_id text COLLATE pg_catalog."default" NOT NULL,
    daily_cooldown timestamp without time zone,
    work_cooldown timestamp without time zone,
    CONSTRAINT user_cooldowns_pkey PRIMARY KEY (user_id),
    CONSTRAINT user_cooldowns_user_id_fkey FOREIGN KEY (user_id)
        REFERENCES public.users (user_id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public.user_cooldowns
    OWNER to inferior_user;