DROP POLICY tenant_isolation ON "LocationServiceOverrides";
ALTER TABLE "LocationServiceOverrides" DISABLE ROW LEVEL SECURITY;

DROP POLICY tenant_isolation ON "ScheduleExceptions";
ALTER TABLE "ScheduleExceptions" DISABLE ROW LEVEL SECURITY;

DROP POLICY tenant_isolation ON "RecurringSchedules";
ALTER TABLE "RecurringSchedules" DISABLE ROW LEVEL SECURITY;

DROP POLICY tenant_isolation ON "EmployeeLocationAssignments";
ALTER TABLE "EmployeeLocationAssignments" DISABLE ROW LEVEL SECURITY;

DROP POLICY tenant_isolation ON "Reviews";
ALTER TABLE "Reviews" DISABLE ROW LEVEL SECURITY;

DROP POLICY tenant_isolation ON "WaitlistEntries";
ALTER TABLE "WaitlistEntries" DISABLE ROW LEVEL SECURITY;

DROP POLICY tenant_isolation ON "Bookings";
ALTER TABLE "Bookings" DISABLE ROW LEVEL SECURITY;

DROP POLICY tenant_isolation ON "Services";
ALTER TABLE "Services" DISABLE ROW LEVEL SECURITY;

DROP POLICY tenant_isolation ON "Employees";
ALTER TABLE "Employees" DISABLE ROW LEVEL SECURITY;

DROP POLICY tenant_isolation ON "OrganizationMembers";
DROP POLICY member_self_read ON "OrganizationMembers";
ALTER TABLE "OrganizationMembers" DISABLE ROW LEVEL SECURITY;

DROP POLICY tenant_isolation ON "Locations";
ALTER TABLE "Locations" DISABLE ROW LEVEL SECURITY;
