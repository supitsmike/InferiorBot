-- Table: public.jobs

-- DROP TABLE IF EXISTS public.jobs;

CREATE TABLE IF NOT EXISTS public.jobs
(
    job_id integer NOT NULL GENERATED ALWAYS AS IDENTITY ( INCREMENT 1 START 1 MINVALUE 1 MAXVALUE 2147483647 CACHE 1 ),
    job_title text COLLATE pg_catalog."default" NOT NULL,
    pay_min money NOT NULL,
    pay_max money NOT NULL,
    cooldown integer NOT NULL,
    risk_level smallint NOT NULL,
    probability double precision NOT NULL,
    CONSTRAINT jobs_pkey PRIMARY KEY (job_id)
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public.jobs
    OWNER to inferior_user;