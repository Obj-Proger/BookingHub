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

**PostgreSQL on another machine instead of the compose container:** create an empty `bookinghub`
database there, run the same two steps against it with a superuser of that server, and point the
API at it without committing anything:

    dotnet user-secrets set "ConnectionStrings:Database" "Host=<server>;Port=5432;Database=bookinghub;Username=bookinghub_app;Password=<password>" --project src/BookingHub.API

Optionally fill the empty database with demo data — an organization with two locations, staff
accounts for every role, services, schedules, bookings and reviews. It prints the accounts and a
few guest links to try:

    dotnet run --project src/BookingHub.API -- seed

## 3. Run

    dotnet run --project src/BookingHub.API    # https://localhost:7117, Scalar UI at /scalar/v1
    dotnet run --project src/BookingHub.Web    # https://localhost:7201

In Development, emails and SMS are not sent: their text, including every link, is written to the
API's console output.

## 4. Reset the database

    docker compose down -v
    docker compose up -d

Then repeat the `dotnet ef database update` and `ALTER ROLE` steps (and the seed) from section 2.

To check the whole application by hand, follow [docs/manual-testing.md](docs/manual-testing.md).

## 5. Tests

    dotnet test --project tests/BookingHub.Domain.Tests
    dotnet test --project tests/BookingHub.Application.Tests
    dotnet test --project tests/BookingHub.Mobile.Tests
    dotnet test --project tests/BookingHub.Infrastructure.IntegrationTests
    dotnet test --project tests/BookingHub.API.Tests

The last two need PostgreSQL. By default each test run starts a throwaway PostgreSQL 18 container
through Testcontainers, which needs Docker on this machine. To use an existing PostgreSQL server
instead, give the tests a superuser connection string; every run then creates its own
`bookinghub_test_*` database and login on that server and drops them when it finishes:

    $env:BOOKINGHUB_TEST_POSTGRES = "Host=<server>;Port=5432;Database=postgres;Username=<superuser>;Password=<password>"

A run that is killed halfway leaves its `bookinghub_test_*` database and login behind; drop them by
hand (`\l` and `\du` in psql list them).
