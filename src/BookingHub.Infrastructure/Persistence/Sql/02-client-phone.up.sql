-- One client per phone number. EF Core cannot declare an index on a complex-type property
-- (dotnet/efcore#32578), so it is created here, on the column ClientConfiguration maps.
CREATE UNIQUE INDEX "IX_Clients_Phone" ON "Clients" ("Phone");
