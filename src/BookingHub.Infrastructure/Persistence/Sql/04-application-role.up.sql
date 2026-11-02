-- The unprivileged role the application connects as. Migrations run as a superuser (or the
-- database owner), which bypasses row-level security; bookinghub_app does not, so every query the
-- application makes is subject to the policies above. The password is set outside the migration
-- (see README), so no secret is committed. Roles are cluster-wide: the role may already exist from
-- another database on the same server.
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'bookinghub_app') THEN
        CREATE ROLE bookinghub_app WITH LOGIN;
    END IF;
END
$$;

GRANT USAGE ON SCHEMA public TO bookinghub_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO bookinghub_app;
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO bookinghub_app;

-- Hangfire creates its own schema on first start, connected as this role.
DO $$
BEGIN
    EXECUTE format('GRANT CREATE ON DATABASE %I TO bookinghub_app', current_database());
END
$$;
