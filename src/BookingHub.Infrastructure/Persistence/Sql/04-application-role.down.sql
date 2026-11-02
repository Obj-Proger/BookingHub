-- The role itself is kept: it is cluster-wide and may still be used by another database.
DO $$
BEGIN
    EXECUTE format('REVOKE CREATE ON DATABASE %I FROM bookinghub_app', current_database());
END
$$;

ALTER DEFAULT PRIVILEGES IN SCHEMA public REVOKE SELECT, INSERT, UPDATE, DELETE ON TABLES FROM bookinghub_app;
REVOKE ALL ON ALL TABLES IN SCHEMA public FROM bookinghub_app;
REVOKE USAGE ON SCHEMA public FROM bookinghub_app;
