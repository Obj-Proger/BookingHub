-- A confirmed booking may not overlap another confirmed booking of the same employee.
-- The last line of defence against double booking: two requests that both passed the
-- availability check can still race, and the loser fails here (ConcurrencyConflictException).
CREATE EXTENSION IF NOT EXISTS btree_gist;

ALTER TABLE "Bookings" ADD CONSTRAINT "EX_Bookings_NoOverlapPerEmployee"
    EXCLUDE USING gist (
        "EmployeeId" WITH =,
        tstzrange("TimeSlot_StartUtc", "TimeSlot_EndUtc", '[)') WITH &&
    )
    WHERE ("Status" = 'Confirmed');
