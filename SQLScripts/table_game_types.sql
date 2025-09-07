-- Table: public.game_types

-- DROP TABLE IF EXISTS public.game_types;

CREATE TABLE IF NOT EXISTS public.game_types
(
    game_type_id uuid NOT NULL DEFAULT uuid_generate_v7(),
    name text COLLATE pg_catalog."default" NOT NULL,
    enabled boolean NOT NULL DEFAULT false,
    CONSTRAINT game_types_pkey PRIMARY KEY (game_type_id),
    CONSTRAINT game_types_name_key UNIQUE (name)
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public.game_types
    OWNER to inferior_user;