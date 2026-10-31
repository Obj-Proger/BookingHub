# BookingHub — Getting Started (local development)

Requirements: .NET 10 SDK, Docker Desktop. All commands run from the repository root.

## 1. Start Postgres

    docker compose up -d

The container listens on port **5433**, not the Postgres default 5432, so it does not clash with a
natively installed Postgres.

## 2. One-time setup

Restore the local tools (`dotnet-ef`) and set the only secret the API needs — the JWT signing key
(any random string of at least 32 characters):

    dotnet tool restore
    dotnet user-secrets set "Jwt:SigningKey" "<random string, 32+ characters>" --project src/BookingHub.API

Everything else for local development (database connection, Web/API URLs, log-only notifications)
is already in the committed `appsettings.Development.json` files.

Create the database. Migrations run as the `postgres` superuser, because they create the
unprivileged `bookinghub_app` role the API itself connects as:

    dotnet ef database update --project src/BookingHub.Infrastructure --connection "Host=localhost;Port=5433;Database=bookinghub;Username=postgres;Password=postgres"

Then give that role the password `appsettings.Development.json` expects:

    docker exec -it bookinghub-pg psql -U postgres -d bookinghub -c "ALTER ROLE bookinghub_app WITH PASSWORD 'bookinghub_app_password';"

## 3. Run

    dotnet run --project src/BookingHub.API    # https://localhost:7117, Scalar UI at /scalar/v1
    dotnet run --project src/BookingHub.Web    # https://localhost:7201

In Development, emails and SMS are not sent: their text, including every link, is written to the
API's console output.

## 4. Reset the database

    docker compose down -v
    docker compose up -d

Then repeat the `dotnet ef database update` and `ALTER ROLE` steps from section 2.
