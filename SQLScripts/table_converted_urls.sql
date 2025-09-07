-- Table: public.converted_urls

-- DROP TABLE IF EXISTS public.converted_urls;

CREATE TABLE IF NOT EXISTS public.converted_urls
(
    guild_id text COLLATE pg_catalog."default" NOT NULL,
    channel_id text COLLATE pg_catalog."default" NOT NULL,
    message_id text COLLATE pg_catalog."default" NOT NULL,
    user_id text COLLATE pg_catalog."default" NOT NULL,
    original_url text COLLATE pg_catalog."default" NOT NULL,
    date_posted timestamp without time zone NOT NULL,
    CONSTRAINT converted_urls_pkey PRIMARY KEY (guild_id, channel_id, message_id),
    CONSTRAINT converted_urls_guild_id_fkey FOREIGN KEY (guild_id)
        REFERENCES public.guilds (guild_id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION,
    CONSTRAINT converted_urls_user_id_fkey FOREIGN KEY (user_id)
        REFERENCES public.users (user_id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public.converted_urls
    OWNER to inferior_user;