-- Table: public.guilds

-- DROP TABLE IF EXISTS public.guilds;

CREATE TABLE IF NOT EXISTS public.guilds
(
    guild_id text COLLATE pg_catalog."default" NOT NULL,
    bot_channels text[] COLLATE pg_catalog."default" NOT NULL DEFAULT ARRAY[]::text[],
    dj_roles text[] COLLATE pg_catalog."default" NOT NULL DEFAULT ARRAY[]::text[],
    convert_urls boolean NOT NULL DEFAULT false,
    CONSTRAINT guilds_pkey PRIMARY KEY (guild_id),
    CONSTRAINT guilds_guild_id_key UNIQUE (guild_id)
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public.guilds
    OWNER to inferior_user;