-- Row-level security: the rows of an organization are visible only inside its tenant scope, which
-- RowLevelSecuritySession sets per transaction (app.current_organization_id). Outside a scope the
-- setting is empty, NULLIF turns it into NULL, and no row matches.
-- Organizations (the tenant root), Clients (shared across organizations), the Identity tables,
-- RefreshTokens and OutboxMessages have no organization scope and no policy.

ALTER TABLE "Locations" ENABLE ROW LEVEL SECURITY;
CREATE POLICY tenant_isolation ON "Locations"
    USING ("OrganizationId" = NULLIF(current_setting('app.current_organization_id', true), '')::uuid);

ALTER TABLE "OrganizationMembers" ENABLE ROW LEVEL SECURITY;
CREATE POLICY tenant_isolation ON "OrganizationMembers"
    USING ("OrganizationId" = NULLIF(current_setting('app.current_organization_id', true), '')::uuid);

-- A signed-in user may read their own memberships in every organization (the /me routes).
-- Permissive policies are OR-ed, and a FOR SELECT policy never allows writes.
CREATE POLICY member_self_read ON "OrganizationMembers" FOR SELECT
    USING ("UserId" = NULLIF(current_setting('app.current_user_id', true), '')::uuid);

ALTER TABLE "Employees" ENABLE ROW LEVEL SECURITY;
CREATE POLICY tenant_isolation ON "Employees"
    USING ("OrganizationId" = NULLIF(current_setting('app.current_organization_id', true), '')::uuid);

ALTER TABLE "Services" ENABLE ROW LEVEL SECURITY;
CREATE POLICY tenant_isolation ON "Services"
    USING ("OrganizationId" = NULLIF(current_setting('app.current_organization_id', true), '')::uuid);

ALTER TABLE "Bookings" ENABLE ROW LEVEL SECURITY;
CREATE POLICY tenant_isolation ON "Bookings"
    USING ("OrganizationId" = NULLIF(current_setting('app.current_organization_id', true), '')::uuid);

ALTER TABLE "WaitlistEntries" ENABLE ROW LEVEL SECURITY;
CREATE POLICY tenant_isolation ON "WaitlistEntries"
    USING ("OrganizationId" = NULLIF(current_setting('app.current_organization_id', true), '')::uuid);

ALTER TABLE "Reviews" ENABLE ROW LEVEL SECURITY;
CREATE POLICY tenant_isolation ON "Reviews"
    USING ("OrganizationId" = NULLIF(current_setting('app.current_organization_id', true), '')::uuid);

-- Tables without their own OrganizationId reach it through the location they belong to.
ALTER TABLE "EmployeeLocationAssignments" ENABLE ROW LEVEL SECURITY;
CREATE POLICY tenant_isolation ON "EmployeeLocationAssignments"
    USING (EXISTS (
        SELECT 1 FROM "Locations" l
        WHERE l."Id" = "LocationId" AND l."OrganizationId" = NULLIF(current_setting('app.current_organization_id', true), '')::uuid
    ));

ALTER TABLE "RecurringSchedules" ENABLE ROW LEVEL SECURITY;
CREATE POLICY tenant_isolation ON "RecurringSchedules"
    USING (EXISTS (
        SELECT 1 FROM "EmployeeLocationAssignments" a
        JOIN "Locations" l ON l."Id" = a."LocationId"
        WHERE a."Id" = "EmployeeLocationAssignmentId" AND l."OrganizationId" = NULLIF(current_setting('app.current_organization_id', true), '')::uuid
    ));

ALTER TABLE "ScheduleExceptions" ENABLE ROW LEVEL SECURITY;
CREATE POLICY tenant_isolation ON "ScheduleExceptions"
    USING (EXISTS (
        SELECT 1 FROM "EmployeeLocationAssignments" a
        JOIN "Locations" l ON l."Id" = a."LocationId"
        WHERE a."Id" = "EmployeeLocationAssignmentId" AND l."OrganizationId" = NULLIF(current_setting('app.current_organization_id', true), '')::uuid
    ));

ALTER TABLE "LocationServiceOverrides" ENABLE ROW LEVEL SECURITY;
CREATE POLICY tenant_isolation ON "LocationServiceOverrides"
    USING (EXISTS (
        SELECT 1 FROM "Locations" l
        WHERE l."Id" = "LocationId" AND l."OrganizationId" = NULLIF(current_setting('app.current_organization_id', true), '')::uuid
    ));
