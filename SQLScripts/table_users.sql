-- Table: public.users

-- DROP TABLE IF EXISTS public.users;

CREATE TABLE IF NOT EXISTS public.users
(
    user_id numeric(19,0) NOT NULL,
    balance money NOT NULL DEFAULT 100,
    banned boolean NOT NULL DEFAULT false,
    daily_cooldown timestamp without time zone,
    CONSTRAINT users_pkey PRIMARY KEY (user_id),
    CONSTRAINT users_user_id_key UNIQUE (user_id)
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public.users
    OWNER to inferior_user;