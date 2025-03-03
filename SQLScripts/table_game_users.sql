-- Table: public.game_users

-- DROP TABLE IF EXISTS public.game_users;

CREATE TABLE IF NOT EXISTS public.game_users
(
    game_id uuid NOT NULL,
    user_id numeric(19,0) NOT NULL,
    user_data json,
    CONSTRAINT game_users_pkey PRIMARY KEY (game_id, user_id),
    CONSTRAINT game_users_game_id_user_id_key UNIQUE (game_id, user_id),
    CONSTRAINT game_users_game_id_fkey FOREIGN KEY (game_id)
        REFERENCES public.games (game_id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
        NOT VALID,
    CONSTRAINT game_users_user_id_fkey FOREIGN KEY (user_id)
        REFERENCES public.users (user_id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public.game_users
    OWNER to inferior_user;