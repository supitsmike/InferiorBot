-- Table: public.games

-- DROP TABLE IF EXISTS public.games;

CREATE TABLE IF NOT EXISTS public.games
(
    game_id uuid NOT NULL DEFAULT uuid_generate_v7(),
    game_type_id uuid NOT NULL,
    guild_id text COLLATE pg_catalog."default",
    user_id text COLLATE pg_catalog."default",
    game_data json NOT NULL,
    created_date timestamp without time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT games_pkey PRIMARY KEY (game_id),
    CONSTRAINT games_game_type_id_fkey FOREIGN KEY (game_type_id)
        REFERENCES public.game_types (game_type_id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION,
    CONSTRAINT games_guild_id_fkey FOREIGN KEY (guild_id)
        REFERENCES public.guilds (guild_id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION,
    CONSTRAINT games_user_id_fkey FOREIGN KEY (user_id)
        REFERENCES public.users (user_id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public.games
    OWNER to inferior_user;