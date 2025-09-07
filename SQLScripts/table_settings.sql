-- Table: public.settings

-- DROP TABLE IF EXISTS public.settings;

CREATE TABLE IF NOT EXISTS public.settings
(
    setting_id uuid NOT NULL DEFAULT uuid_generate_v7(),
    name text COLLATE pg_catalog."default" NOT NULL,
    value text COLLATE pg_catalog."default" NOT NULL,
    CONSTRAINT settings_pkey PRIMARY KEY (setting_id)
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public.settings
    OWNER to inferior_user;