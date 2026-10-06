# BookingHub — Getting Started (local development)

## 1. Start Postgres

    docker compose up -d

(fixed on port **5433**, not the Postgres default 5432 — avoids clashing with any natively installed Postgres)

## 2. One-time setup

    dotnet user-secrets init --project src/BookingHub.API
    dotnet user-secrets set "ConnectionStrings:Database" "Host=localhost;Port=5433;Database=bookinghub;Username=bookinghub_app;Password=bookinghub_app_password" --project src/BookingHub.API

    dotnet ef database update --project src/BookingHub.Infrastructure --startup-project src/BookingHub.API --connection "Host=localhost;Port=5433;Database=bookinghub;Username=postgres;Password=postgres"

Then set a password for the app role the migration created (any Postgres client, e.g. `psql -h localhost -p 5433 -U postgres -d bookinghub`):

    ALTER ROLE bookinghub_app WITH PASSWORD 'bookinghub_app_password';

## 3. Run

    dotnet run --project src/BookingHub.API    # https://localhost:7117, Scalar UI at /scalar/v1
    dotnet run --project src/BookingHub.Web    # https://localhost:7201

## 4. Reset the database

    docker compose down -v && docker compose up -d
    dotnet ef database update --project src/BookingHub.Infrastructure --startup-project src/BookingHub.API --connection "Host=localhost;Port=5433;Database=bookinghub;Username=postgres;Password=postgres"
    # then re-run the ALTER ROLE step above