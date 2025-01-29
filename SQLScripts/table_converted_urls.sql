-- Table: public.converted_urls

-- DROP TABLE IF EXISTS public.converted_urls;

CREATE TABLE IF NOT EXISTS public.converted_urls
(
    message_id numeric(19,0) NOT NULL,
    user_id numeric(19,0) NOT NULL,
    original_url character varying(2048) COLLATE pg_catalog."default" NOT NULL,
    converted_url character varying(2048) COLLATE pg_catalog."default" NOT NULL,
    date_posted date NOT NULL,
    CONSTRAINT converted_urls_pkey PRIMARY KEY (message_id),
    CONSTRAINT converted_urls_message_id_key UNIQUE (message_id),
    CONSTRAINT converted_urls_user_id_fkey FOREIGN KEY (user_id)
        REFERENCES public.users (user_id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public.converted_urls
    OWNER to inferior_user;