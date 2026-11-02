-- The unprivileged role the application connects as. Migrations run as a superuser (or the
-- database owner), which bypasses row-level security; bookinghub_app does not, so every query the
-- application makes is subject to the policies above. The password is set outside the migration
-- (see README), so no secret is committed. Roles are cluster-wide: the role may already exist from
-- another database on the same server, or be created this very moment by a migration of another
-- database running alongside (parallel test runs). Checking first and creating second would race,
-- so the role is created outright and "already exists" (a duplicate or, mid-race, a unique
-- violation in the role catalog) is accepted.
DO $$
BEGIN
    CREATE ROLE bookinghub_app WITH LOGIN;
EXCEPTION
    WHEN duplicate_object OR unique_violation THEN
        NULL;
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
