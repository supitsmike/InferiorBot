-- Table: public.guilds

-- DROP TABLE IF EXISTS public.guilds;

CREATE TABLE IF NOT EXISTS public.guilds
(
    guild_id numeric(19,0) NOT NULL,
    bot_channels numeric(19,0)[] NOT NULL DEFAULT ARRAY[]::numeric[],
    dj_roles numeric(19,0)[] NOT NULL DEFAULT ARRAY[]::numeric[],
    convert_urls boolean NOT NULL DEFAULT false,
    CONSTRAINT guilds_pkey PRIMARY KEY (guild_id),
    CONSTRAINT guilds_guild_id_key UNIQUE (guild_id)
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public.guilds
    OWNER to inferior_user;